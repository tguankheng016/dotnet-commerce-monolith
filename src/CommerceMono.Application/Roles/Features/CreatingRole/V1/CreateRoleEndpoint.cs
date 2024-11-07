using CommerceMono.Application.Data;
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

namespace CommerceMono.Application.Roles.Features.CreatingRole.V1;

public class CreateRoleEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapPost($"{EndpointConfig.BaseApiPath}/role", Handle)
			.RequireAuthorization(RolePermissions.Pages_Administration_Roles_Create)
			.WithName("CreateRole")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.Produces<CreateRoleResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Create New Role")
			.WithDescription("Create New Role")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
			IMediator mediator, CancellationToken cancellationToken,
			[FromBody] CreateRoleDto request
		)
	{
		var mapper = new RoleMapper();
		var command = mapper.CreateRoleDtoToCreateRoleCommand(request);

		var result = await mediator.Send(command, cancellationToken);

		return Results.Ok(result);
	}
}

// Result
public record CreateRoleResult(RoleDto Role);

// Command
public class CreateRoleCommand : CreateRoleDto, ICommand<CreateRoleResult>, ITransactional;

// Validator
public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
	public CreateRoleValidator()
	{
		RuleFor(x => x.Id).Must(x => x is null || x == 0).WithMessage("Invalid role id");
		RuleFor(x => x.Name).NotEmpty().WithMessage("Please enter the name");
	}
}

// Handler
internal class CreateRoleHandler(
	RoleManager<Role> roleManager,
	IUserRolePermissionManager userRolePermissionManager,
	AppDbContext appDbContext
) : ICommandHandler<CreateRoleCommand, CreateRoleResult>
{
	public async Task<CreateRoleResult> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
	{
		var mapper = new RoleMapper();
		var role = mapper.CreateRoleDtoToRole(command);

		var roleResult = await roleManager.CreateAsync(role);

		if (!roleResult.Succeeded)
		{
			throw new BadRequestException(string.Join(',', roleResult.Errors.Select(e => e.Description)));
		}

		userRolePermissionManager.ValidatePermissions(command.GrantedPermissions);

		foreach (var permission in command.GrantedPermissions)
		{
			await appDbContext.UserRolePermissions.AddAsync(new UserRolePermission()
			{
				Id = 0,
				RoleId = role.Id,
				Name = permission,
				IsGranted = true
			}, cancellationToken);
		}

		var roleDto = mapper.RoleToRoleDto(role);

		return new CreateRoleResult(roleDto);
	}
}