using CommerceMono.Application.Users.Constants;
using CommerceMono.Application.Users.Models;
using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Core.CQRS;
using CommerceMono.Modules.Core.EFCore;
using CommerceMono.Modules.Core.Exceptions;
using CommerceMono.Modules.Core.Sessions;
using CommerceMono.Modules.Permissions;
using CommerceMono.Modules.Security.Caching;
using CommerceMono.Modules.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CommerceMono.Application.Users.Features.DeletingUser.V1;

public class DeleteUserEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapDelete($"{EndpointConfig.BaseApiPath}/user/{{userid:long}}", Handle)
			.RequireAuthorization(UserPermissions.Pages_Administration_Users_Delete)
			.WithName("DeleteUser")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Delete User")
			.WithDescription("Delete User")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator, CancellationToken cancellationToken,
		[FromRoute] long UserId
	)
	{
		var command = new DeleteUserCommand(UserId);

		await mediator.Send(command, cancellationToken);

		return Results.Ok();
	}
}

// Result
public record DeleteUserResult();

// Command
public record DeleteUserCommand(long Id) : ICommand<DeleteUserResult>, ITransactional;

// Validator
public class DeleteUserValidator : AbstractValidator<DeleteUserCommand>
{
	public DeleteUserValidator()
	{
		RuleFor(x => x.Id).GreaterThan(0).WithMessage("Invalid user id");
	}
}

// Handler
internal class DeleteUserHandler(
	UserManager<User> userManager,
	IAppSession appSession,
	ICacheManager cacheManager
) : ICommandHandler<DeleteUserCommand, DeleteUserResult>
{
	public async Task<DeleteUserResult> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
	{
		var user = await userManager.FindByIdAsync(command.Id.ToString());

		if (user is null)
		{
			throw new NotFoundException("User not found");
		}

		if (user.UserName == UserConsts.DefaultUsername.Admin)
		{
			throw new BadRequestException("You cannot delete admin account!");
		}

		if (user.Id == appSession.UserId)
		{
			throw new BadRequestException("You cannot delete your own account!");
		}

		// TODO: Temporary Solution because of unique index username and soft delete issue
		user.UserName += "_DELETED";
		await userManager.UpdateAsync(user);

		await userManager.DeleteAsync(user);

		var _cacheProvider = cacheManager.GetCachingProvider();

		// Invalidate Deleted User Tokens
		await _cacheProvider.RemoveAsync(SecurityStampCacheItem.GenerateCacheKey(user.Id.ToString()), cancellationToken);

		return new DeleteUserResult();
	}
}
