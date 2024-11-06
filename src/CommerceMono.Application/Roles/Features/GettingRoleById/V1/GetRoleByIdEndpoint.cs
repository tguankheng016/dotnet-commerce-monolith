using CommerceMono.Application.Roles.Dtos;
using CommerceMono.Application.Roles.Models;
using CommerceMono.Application.Users.Services;
using CommerceMono.Modules.Core.CQRS;
using CommerceMono.Modules.Core.Exceptions;
using CommerceMono.Modules.Permissions;
using CommerceMono.Modules.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CommerceMono.Application.Roles.Features.GettingRoleById.V1;

public class GetRoleByIdEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapGet($"{EndpointConfig.BaseApiPath}/role/{{roleid:long}}", Handle)
			.RequireAuthorization(RolePermissions.Pages_Administration_Roles)
			.WithName("GetRoleById")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.Produces<GetRoleByIdResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Get Role By Id")
			.WithDescription("Get Role By Id")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator, CancellationToken cancellationToken,
		[FromRoute] long roleId
	)
	{
		var query = new GetRoleByIdQuery(roleId);

		var result = await mediator.Send(query, cancellationToken);

		return Results.Ok(result);
	}
}

// Result
public record GetRoleByIdResult(CreateOrEditRoleDto Role);

// Query
public record GetRoleByIdQuery(long Id) : IQuery<GetRoleByIdResult>;

// Handler
internal class GetRoleByIdHandler(
	RoleManager<Role> roleManager,
	IUserRolePermissionManager userRolePermissionManager
) : IQueryHandler<GetRoleByIdQuery, GetRoleByIdResult>
{
	public async Task<GetRoleByIdResult> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
	{
		CreateOrEditRoleDto roleEditDto;

		if (request.Id == 0)
		{
			// Create
			roleEditDto = new CreateOrEditRoleDto();
		}
		else
		{
			// Edit
			var role = await roleManager.Roles
				.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);

			if (role is null)
			{
				throw new NotFoundException("Role not found");
			}

			var mapper = new RoleMapper();

			roleEditDto = mapper.RoleToCreateOrEditRoleDto(role);

			roleEditDto.GrantedPermissions = (await userRolePermissionManager
				.SetRolePermissionAsync(role.Id, cancellationToken))
				.Select(x => x.Key)
				.ToList();
		}

		return new GetRoleByIdResult(roleEditDto);
	}
}