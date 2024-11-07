using CommerceMono.Application.Data;
using CommerceMono.Application.Roles.Constants;
using CommerceMono.Application.Roles.Dtos;
using CommerceMono.Application.Roles.Models;
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

namespace CommerceMono.Application.Roles.Features.UpdatingRole.V1;

public class UpdateRoleEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapPut($"{EndpointConfig.BaseApiPath}/role", Handle)
			.RequireAuthorization(RolePermissions.Pages_Administration_Roles_Edit)
			.WithName("UpdateRole")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.Produces<UpdateRoleResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Update New Role")
			.WithDescription("Update New Role")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator, CancellationToken cancellationToken,
		[FromBody] EditRoleDto request
	)
	{
		var mapper = new RoleMapper();
		var command = mapper.EditRoleDtoToUpdateRoleCommand(request);

		var result = await mediator.Send(command, cancellationToken);

		return Results.Ok(result);
	}
}

// Result
public record UpdateRoleResult(RoleDto Role);

// Command
public class UpdateRoleCommand : EditRoleDto, ICommand<UpdateRoleResult>, ITransactional;

// Validator
public class UpdateRoleValidator : AbstractValidator<UpdateRoleCommand>
{
	public UpdateRoleValidator()
	{
		RuleFor(x => x.Id).NotEmpty().WithMessage("Invalid role id");
		RuleFor(x => x.Id).GreaterThan(0).WithMessage("Invalid role id");
		RuleFor(x => x.Name).NotEmpty().WithMessage("Please enter the name");
	}
}

// Handler
internal class UpdateRoleHandler(
	RoleManager<Role> roleManager,
	IUserRolePermissionManager userRolePermissionManager,
	AppDbContext appDbContext
) : ICommandHandler<UpdateRoleCommand, UpdateRoleResult>
{
	public async Task<UpdateRoleResult> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
	{
		var role = await roleManager.FindByIdAsync(command.Id!.ToString()!);

		if (role is null)
		{
			throw new NotFoundException("Role not found");
		}

		if (role.IsStatic && role.NormalizedName != command.Name.ToUpper())
		{
			throw new BadRequestException("You cannot change the name of static role");
		}

		var mapper = new RoleMapper();

		mapper.EditRoleDtoToRole(command, role);

		var roleResult = await roleManager.UpdateAsync(role);

		if (!roleResult.Succeeded)
		{
			throw new BadRequestException(string.Join(',', roleResult.Errors.Select(e => e.Description)));
		}

		userRolePermissionManager.ValidatePermissions(command.GrantedPermissions);

		var oldPermissions = await userRolePermissionManager.SetRolePermissionAsync(role.Id, cancellationToken);

		var newPermissions = command.GrantedPermissions.ToArray();

		var isAdmin = role.NormalizedName == RoleConsts.RoleName.Admin.ToUpper();

		// Prohibit
		foreach (var permission in oldPermissions.Where(p => !newPermissions.Contains(p.Key)))
		{
			var rolePermissionToRemoved = await appDbContext.UserRolePermissions
				.FirstOrDefaultAsync(x => x.RoleId == role.Id && x.Name == permission.Key, cancellationToken);

			if (rolePermissionToRemoved is not null)
			{
				appDbContext.UserRolePermissions.Remove(rolePermissionToRemoved);
			}

			if (isAdmin)
			{
				// Admin need to set is granted to false
				await appDbContext.UserRolePermissions.AddAsync(new UserRolePermission()
				{
					Id = 0,
					RoleId = role.Id,
					Name = permission.Key,
					IsGranted = false
				}, cancellationToken);

				await appDbContext.SaveChangesAsync(cancellationToken);
			}
		}

		// Granted
		foreach (var permission in newPermissions.Where(p => !oldPermissions.ContainsKey(p)))
		{
			var rolePermissionToGranted = await appDbContext.UserRolePermissions
				.FirstOrDefaultAsync(x => x.RoleId == role.Id && x.Name == permission, cancellationToken);

			if (rolePermissionToGranted is null)
			{
				await appDbContext.UserRolePermissions.AddAsync(new UserRolePermission()
				{
					Id = 0,
					RoleId = role.Id,
					Name = permission,
					IsGranted = true
				}, cancellationToken);

				await appDbContext.SaveChangesAsync(cancellationToken);
			}
			else if (!rolePermissionToGranted.IsGranted)
			{
				if (isAdmin)
				{
					appDbContext.UserRolePermissions.Remove(rolePermissionToGranted);
				}
				else
				{
					// Unlikely will happen
					rolePermissionToGranted.IsGranted = true;
					await appDbContext.SaveChangesAsync(cancellationToken);
				}
			}
		}

		var roleDto = mapper.RoleToRoleDto(role);

		// Reset Role Permission Cache
		await userRolePermissionManager.SetRolePermissionAsync(role.Id, cancellationToken);

		return new UpdateRoleResult(roleDto);
	}
}