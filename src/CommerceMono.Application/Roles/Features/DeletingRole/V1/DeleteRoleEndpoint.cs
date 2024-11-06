using CommerceMono.Application.Roles.Models;
using CommerceMono.Application.Users.Models;
using CommerceMono.Application.Users.Services;
using CommerceMono.Modules.Core.CQRS;
using CommerceMono.Modules.Core.EFCore;
using CommerceMono.Modules.Core.Exceptions;
using CommerceMono.Modules.Permissions;
using CommerceMono.Modules.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CommerceMono.Application.Roles.Features.DeletingRole.V1;

public class DeleteRoleEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapDelete($"{EndpointConfig.BaseApiPath}/role/{{roleid:long}}", Handle)
			.RequireAuthorization(RolePermissions.Pages_Administration_Roles_Delete)
			.WithName("DeleteRole")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Delete Role")
			.WithDescription("Delete Role")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator, CancellationToken cancellationToken,
		[FromRoute] long RoleId
	)
	{
		var command = new DeleteRoleCommand(RoleId);

		await mediator.Send(command, cancellationToken);

		return Results.Ok();
	}
}

// Result
public record DeleteRoleResult();

// Command
public record DeleteRoleCommand(long Id) : ICommand<DeleteRoleResult>, ITransactional;

// Handler
internal class DeleteRoleHandler(
	RoleManager<Role> roleManager,
	UserManager<User> userManager,
	IUserRolePermissionManager userRolePermissionManager
) : ICommandHandler<DeleteRoleCommand, DeleteRoleResult>
{
	public async Task<DeleteRoleResult> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
	{
		var role = await roleManager.FindByIdAsync(command.Id.ToString());

		if (role == null)
		{
			throw new NotFoundException("Role not found");
		}

		if (role.IsStatic)
		{
			throw new BadRequestException("You cannot delete static role!");
		}

		var users = await userManager.GetUsersInRoleAsync(role.Name!);

		foreach (var user in users)
		{
			var removeUserRoleResult = await userManager.RemoveFromRoleAsync(user, role.Name!);

			if (!removeUserRoleResult.Succeeded)
			{
				throw new BadRequestException(string.Join(',', removeUserRoleResult.Errors.Select(e => e.Description)));
			}

			await userRolePermissionManager.RemoveUserRoleCacheAsync(user.Id, cancellationToken);
		}

		var roleDeleteResult = await roleManager.DeleteAsync(role);

		if (!roleDeleteResult.Succeeded)
		{
			throw new BadRequestException(string.Join(',', roleDeleteResult.Errors.Select(e => e.Description)));
		}

		return new DeleteRoleResult();
	}
}