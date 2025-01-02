using CommerceMono.Application.Data;
using CommerceMono.Application.Users.Models;
using CommerceMono.Application.Users.Services;
using CommerceMono.Modules.Core.CQRS;
using CommerceMono.Modules.Core.EFCore;
using CommerceMono.Modules.Core.Exceptions;
using CommerceMono.Modules.Permissions;
using CommerceMono.Modules.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CommerceMono.Application.Users.Features.UpdatingUserPermissions.V1;

public class UpdateUserPermissionsEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapPut($"{EndpointConfig.BaseApiPath}/user/{{userid:long}}/permissions", Handle)
			.RequireAuthorization(UserPermissions.Pages_Administration_Users_ChangePermissions)
			.WithName("UpdateUserPermissions")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.Produces<UpdateUserPermissionsResult>()
			.ProducesProblem(StatusCodes.Status401Unauthorized)
			.ProducesProblem(StatusCodes.Status404NotFound)
			.WithSummary("Update User Permissions")
			.WithDescription("Update User Permissions")
			.WithOpenApi()
			.HasLatestApiVersion();

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator, CancellationToken cancellationToken,
		[FromRoute] long UserId, [FromBody] List<string> RequestBody
	)
	{
		var command = new UpdateUserPermissionsCommand(UserId, RequestBody);

		await mediator.Send(command, cancellationToken);

		return Results.Ok(new UpdateUserPermissionsResult());
	}
}

// Result
public record UpdateUserPermissionsResult();

// Command
public record UpdateUserPermissionsCommand(long UserId, List<string>? GrantedPermissionNames) : ICommand<UpdateUserPermissionsResult>, ITransactional;

// Validator
public class UpdateUserPermissionsValidator : AbstractValidator<UpdateUserPermissionsCommand>
{
	public UpdateUserPermissionsValidator()
	{
		RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Invalid user id");
		RuleFor(x => x.GrantedPermissionNames).NotNull().WithMessage("Invalid permissions");
	}
}

// Handler
internal class UpdateUserPermissionsHandler(
	UserManager<User> userManager,
	IUserRolePermissionManager userRolePermissionManager,
	AppDbContext appDbContext
) : ICommandHandler<UpdateUserPermissionsCommand, UpdateUserPermissionsResult>
{
	public async Task<UpdateUserPermissionsResult> Handle(UpdateUserPermissionsCommand command, CancellationToken cancellationToken)
	{
		userRolePermissionManager.ValidatePermissions(command.GrantedPermissionNames!);

		var user = await userManager.FindByIdAsync(command.UserId.ToString());

		if (user is null)
		{
			throw new NotFoundException("User not found");
		}

		var roleIds = await appDbContext.UserRoles.AsNoTracking()
			.Where(x => x.UserId == user.Id)
			.Select(x => x.RoleId)
			.ToListAsync(cancellationToken);

		var userRolesPermissions = await GetUserRolesPermissionsAsync(roleIds, cancellationToken);

		var oldPermissions = await userRolePermissionManager.SetUserPermissionAsync(user.Id, cancellationToken);
		var newPermissions = command.GrantedPermissionNames!.ToArray();

		// Prohibit
		foreach (var permission in oldPermissions.Where(p => !newPermissions.Contains(p.Key)))
		{
			var userPermissionToRemoved = await appDbContext.UserRolePermissions
				.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Name == permission.Key, cancellationToken);

			if (userPermissionToRemoved != null)
			{
				appDbContext.UserRolePermissions.Remove(userPermissionToRemoved);
				await appDbContext.SaveChangesAsync(cancellationToken);
			}

			// Skip if role dont have the permission
			if (!userRolesPermissions.ContainsKey(permission.Key))
			{
				continue;
			}

			// Prohibit at user level if role is granted
			await appDbContext.UserRolePermissions.AddAsync(new UserRolePermission()
			{
				UserId = user.Id,
				Name = permission.Key,
				IsGranted = false
			}, cancellationToken);
		}

		// Granted
		foreach (var permission in newPermissions.Where(p => !oldPermissions.ContainsKey(p)))
		{
			// Check is any false granted user level permission
			var userFalseGrantedPermission = await appDbContext.UserRolePermissions
				.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Name == permission && !x.IsGranted, cancellationToken);

			if (userFalseGrantedPermission != null)
			{
				appDbContext.UserRolePermissions.Remove(userFalseGrantedPermission);
			}

			// Skip if role already have that permission
			if (userRolesPermissions.ContainsKey(permission))
			{
				continue;
			}

			// Added at user level if role is not granted
			await appDbContext.UserRolePermissions.AddAsync(new UserRolePermission()
			{
				UserId = user.Id,
				Name = permission,
				IsGranted = true
			}, cancellationToken);
		}

		// Reset User Permission Cache
		await userRolePermissionManager.SetUserPermissionAsync(user.Id, cancellationToken);

		return new UpdateUserPermissionsResult();
	}

	public async Task<Dictionary<string, string>> GetUserRolesPermissionsAsync(List<long>? roleIds, CancellationToken cancellationToken)
	{
		var permissions = new Dictionary<string, string>();

		if (roleIds == null || roleIds.Count == 0)
		{
			return permissions;
		}

		foreach (var roleId in roleIds)
		{
			var rolePermissions = await userRolePermissionManager.SetRolePermissionAsync(roleId, cancellationToken);

			if (rolePermissions is not null)
			{
				foreach (var rolePermission in rolePermissions)
				{
					if (!permissions.ContainsKey(rolePermission.Key))
					{
						permissions.Add(rolePermission.Key, rolePermission.Value);
					}
				}
			}
		}

		return permissions;
	}
}

