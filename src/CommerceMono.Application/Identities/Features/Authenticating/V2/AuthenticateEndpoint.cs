using System.Security.Claims;
using Asp.Versioning.Conventions;
using CommerceMono.Application.Identities.Services;
using CommerceMono.Application.Users.Models;
using CommerceMono.Modules.Core.CQRS;
using CommerceMono.Modules.Core.Exceptions;
using CommerceMono.Modules.Security;
using CommerceMono.Modules.Web;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CommerceMono.Application.Identities.Features.Authenticating.V2;

public class AuthenticateEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapPost($"{EndpointConfig.BaseApiPath}/identities/authenticate", Handle)
			.WithName("AuthenticateV2")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.Produces<AuthenticateResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Authenticate")
			.WithDescription("Authenticate")
			.WithOpenApi()
			.HasApiVersion(2.0);

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator,
		CancellationToken cancellationToken,
		[FromBody] AuthenticateRequest request
	)
	{
		var mapper = new IdentityMapper();
		var command = mapper.AuthenticateRequestToAuthenticateCommand(request);
		var result = await mediator.Send(command, cancellationToken);

		return Results.Ok(result);
	}
}

// Request
public record AuthenticateRequest(
	string? UsernameOrEmailAddress,
	string? Password
);

// Result
public record AuthenticateResult(string AccessToken, int ExpireInSeconds, string RefreshToken, int RefreshTokenExpireInSeconds);

// Command
public record AuthenticateCommand(
	string? UsernameOrEmailAddress,
	string? Password
) : ICommand<AuthenticateResult>
{
}

// Validator
public class AuthenticateValidator : AbstractValidator<AuthenticateCommand>
{
	public AuthenticateValidator()
	{
		RuleFor(x => x.UsernameOrEmailAddress).NotEmpty().WithMessage("Please enter the username or email address");
		RuleFor(x => x.Password).NotEmpty().WithMessage("Please enter the password");
	}
}

// Handler
internal class AuthenticateHandler(
	UserManager<User> userManager,
	SignInManager<User> signInManager,
	IJwtTokenGenerator jwtTokenGenerator,
	IUserClaimsPrincipalFactory<User> userClaimsPrincipal
) : ICommandHandler<AuthenticateCommand, AuthenticateResult>
{
	public async Task<AuthenticateResult> Handle(AuthenticateCommand command, CancellationToken cancellationToken)
	{
		var identityUser = await userManager.FindByNameAsync(command.UsernameOrEmailAddress!)
			?? await userManager.FindByEmailAsync(command.UsernameOrEmailAddress!);

		if (identityUser is null)
		{
			throw new BadRequestException($"Invalid username or password!");
		}

		var signInResult = await signInManager.CheckPasswordSignInAsync(identityUser, command.Password!, false);

		if (signInResult.IsLockedOut)
		{
			throw new BadRequestException($"Your account has been temporarily locked due to multiple unsuccessful login attempts.");
		}

		if (!signInResult.Succeeded)
		{
			throw new BadRequestException($"Invalid username or password!");
		}

		var principal = await userClaimsPrincipal.CreateAsync(identityUser);
		var claimIdentity = principal.Identity as ClaimsIdentity;

		var refreshToken = await jwtTokenGenerator
			.CreateRefreshToken(claimIdentity!, identityUser);

		var accessToken = await jwtTokenGenerator
			.CreateAccessToken(claimIdentity!, identityUser, refreshTokenKey: refreshToken.Key);

		return new AuthenticateResult(
			accessToken,
			(int)TokenConsts.AccessTokenExpiration.TotalSeconds,
			refreshToken.Token,
			(int)TokenConsts.RefreshTokenExpiration.TotalSeconds
		);
	}
}
