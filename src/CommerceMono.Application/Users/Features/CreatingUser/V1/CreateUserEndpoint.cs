using CommerceMono.Application.Roles.Constants;
using CommerceMono.Application.Roles.Models;
using CommerceMono.Application.Users.Dtos;
using CommerceMono.Application.Users.Models;
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

namespace CommerceMono.Application.Users.Features.CreatingUser.V1;

public class CreateUserEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapPost($"{EndpointConfig.BaseApiPath}/user", Handle)
			.RequireAuthorization(UserPermissions.Pages_Administration_Users_Create)
			.WithName("CreateUser")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.Produces<CreateUserResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Create New User")
			.WithDescription("Create New User")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator, CancellationToken cancellationToken,
		[FromBody] CreateUserDto request
	)
	{
		var mapper = new UserMapper();
		var query = mapper.CreateUserDtoToCreateUserCommand(request);

		var result = await mediator.Send(query, cancellationToken);

		return Results.Ok(result);
	}
}

// Result
public record CreateUserResult(UserDto User);

// Command
public class CreateUserCommand : CreateUserDto, ICommand<CreateUserResult>, ITransactional;

// Validator
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
	public CreateUserValidator()
	{
		RuleFor(x => x.Id).Must(x => x is null || x == 0).WithMessage("Invalid user id");
		RuleFor(x => x.Password).NotEmpty().WithMessage("Please enter the password");
		RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("Please enter the confirmation password");

		RuleFor(x => x).Custom((x, context) =>
		{
			if (x.Password != x.ConfirmPassword)
			{
				context.AddFailure(nameof(x.Password), "Passwords should match");
			}
		});

		RuleFor(x => x.UserName).NotEmpty().WithMessage("Please enter the username");
		RuleFor(x => x.FirstName).NotEmpty().WithMessage("Please enter the first name");
		RuleFor(x => x.LastName).NotEmpty().WithMessage("Please enter the last name");
		RuleFor(x => x.Email).NotEmpty().WithMessage("Please enter the email address")
			.EmailAddress().WithMessage("Please enter a valid email address");
		RuleFor(x => x.FirstName).MaximumLength(User.MaxFirstNameLength)
			.WithMessage($"The first name length cannot exceed {User.MaxFirstNameLength} characters.");
		RuleFor(x => x.LastName).MaximumLength(User.MaxLastNameLength)
			.WithMessage($"The first name length cannot exceed {User.MaxLastNameLength} characters.");
	}
}

// Handler
internal class CreateUserHandler(
	UserManager<User> userManager,
	RoleManager<Role> roleManager
) : ICommandHandler<CreateUserCommand, CreateUserResult>
{
	public async Task<CreateUserResult> Handle(CreateUserCommand command, CancellationToken cancellationToken)
	{
		var mapper = new UserMapper();
		var user = mapper.CreateUserDtoToUser(command);

		var users = await userManager.Users.ToListAsync(cancellationToken);

		var identityResult = await userManager.CreateAsync(user, command.Password!);

		if (!identityResult.Succeeded)
		{
			throw new BadRequestException(string.Join(',', identityResult.Errors.Select(e => e.Description)));
		}

		if (command.Roles is null || command.Roles.Count == 0)
		{
			var defaultRole = await roleManager.Roles
				.FirstOrDefaultAsync(x => x.IsDefault, cancellationToken);

			command.Roles = new List<string>
			{
				defaultRole?.Name ?? RoleConsts.RoleName.User
			};
		}

		var roleResult = await userManager.AddToRolesAsync(user, command.Roles);

		if (!roleResult.Succeeded)
		{
			throw new BadRequestException(string.Join(',', roleResult.Errors.Select(e => e.Description)));
		}

		var userDto = mapper.UserToUserDto(user);

		return new CreateUserResult(userDto);
	}
}