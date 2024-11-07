using CommerceMono.Application.Users.Constants;
using CommerceMono.Application.Users.Dtos;
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

namespace CommerceMono.Application.Users.Features.UpdatingUser.V1;

public class UpdateUserEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapPut($"{EndpointConfig.BaseApiPath}/user", Handle)
			.RequireAuthorization(UserPermissions.Pages_Administration_Users_Edit)
			.WithName("UpdateUser")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.Produces<UpdateUserResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Update User")
			.WithDescription("Update User")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator, CancellationToken cancellationToken,
		[FromBody] EditUserDto request
	)
	{
		var mapper = new UserMapper();
		var command = mapper.EdiUserDtoToUpdateUserCommand(request);

		var result = await mediator.Send(command, cancellationToken);

		return Results.Ok(result);
	}
}

// Result
public record UpdateUserResult(UserDto User);

// Command
public class UpdateUserCommand : EditUserDto, ICommand<UpdateUserResult>, ITransactional;

// Validator
public class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
	public UpdateUserValidator()
	{
		RuleFor(x => x.Id).NotEmpty().WithMessage("Invalid user id");
		RuleFor(x => x.Id).GreaterThan(0).WithMessage("Invalid user id");
		RuleFor(x => x).Custom((x, context) =>
		{
			if (!string.IsNullOrEmpty(x.Password) && !string.IsNullOrEmpty(x.ConfirmPassword) && x.Password != x.ConfirmPassword)
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
			.WithMessage($"The last name length cannot exceed {User.MaxLastNameLength} characters.");
	}
}

// Handler
internal class UpdateUserHandler(
	UserManager<User> userManager,
	IUserRolePermissionManager userRolePermissionManager
) : ICommandHandler<UpdateUserCommand, UpdateUserResult>
{
	public async Task<UpdateUserResult> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
	{
		var user = await userManager.FindByIdAsync(command.Id.ToString()!);

		if (user is null)
		{
			throw new NotFoundException("User not found");
		}

		if (user.UserName == UserConsts.DefaultUsername.Admin && command.UserName != user.UserName)
		{
			throw new BadRequestException("You cannot change admin's username");
		}

		var mapper = new UserMapper();
		mapper.EditUserDtoToUser(command, user);

		await userManager.UpdateAsync(user);

		if (!string.IsNullOrEmpty(command.Password))
		{
			var removedPasswordResult = await userManager.RemovePasswordAsync(user);

			if (!removedPasswordResult.Succeeded)
			{
				throw new BadRequestException(string.Join(',', removedPasswordResult.Errors.Select(e => e.Description)));
			}

			var addPasswordResult = await userManager.AddPasswordAsync(user, command.Password);

			if (!addPasswordResult.Succeeded)
			{
				throw new BadRequestException(string.Join(',', addPasswordResult.Errors.Select(e => e.Description)));
			}
		}

		var userRoles = await userManager.GetRolesAsync(user);

		var rolesToAdd = command.Roles.Where(r => !userRoles.Contains(r)).ToList();
		var rolesToRemove = userRoles.Where(r => !command.Roles.Contains(r)).ToList();

		if (rolesToAdd.Count > 0)
		{
			var addRoleResult = await userManager.AddToRolesAsync(user, rolesToAdd);

			if (!addRoleResult.Succeeded)
			{
				throw new BadRequestException(string.Join(',', addRoleResult.Errors.Select(e => e.Description)));
			}
		}

		if (rolesToRemove.Count > 0)
		{
			var removeRoleResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);

			if (!removeRoleResult.Succeeded)
			{
				throw new BadRequestException(string.Join(',', removeRoleResult.Errors.Select(e => e.Description)));
			}
		}

		if (rolesToAdd.Count > 0 || rolesToRemove.Count > 0)
		{
			await userRolePermissionManager.RemoveUserRoleCacheAsync(user.Id, cancellationToken);
		}

		var userDto = mapper.UserToUserDto(user);
		userDto.Roles = await userManager.GetRolesAsync(user);

		return new UpdateUserResult(userDto);
	}
}