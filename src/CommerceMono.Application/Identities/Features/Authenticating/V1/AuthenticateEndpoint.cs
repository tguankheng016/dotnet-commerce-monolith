using System.Security.Claims;
using CommerceMono.Application.Identities.Services;
using CommerceMono.Application.Users.Models;
using CommerceMono.Modules.Core.Exceptions;
using CommerceMono.Modules.Core.Validations;
using CommerceMono.Modules.Security;
using CommerceMono.Modules.Web;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CommerceMono.Application.Identities.Features.Authenticating.V1;

public class AuthenticateEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		// TODO: Add Api Version
		builder.MapPost($"api/identities/authenticate", Handle)
			.WithName("Authenticate")
			//.WithApiVersionSet(builder.NewApiVersionSet("Identities").Build())
			.Produces<AuthenticateResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Authenticate")
			.WithDescription("Authenticate");
		//.WithOpenApi()
		//.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		UserManager<User> userManager,
		SignInManager<User> signInManager,
		IJwtTokenGenerator jwtTokenGenerator,
		IUserClaimsPrincipalFactory<User> userClaimsPrincipal,
		CancellationToken cancellationToken,
		[FromBody] AuthenticateRequest request
	)
	{
		var validator = new AuthenticateValidator();
		await validator.HandleValidationAsync(request);

		var identityUser = await userManager.FindByNameAsync(request.UsernameOrEmailAddress!)
			?? await userManager.FindByEmailAsync(request.UsernameOrEmailAddress!);

		if (identityUser == null)
		{
			throw new BadRequestException($"Invalid username or password!");
		}

		var signInResult = await signInManager.CheckPasswordSignInAsync(identityUser, request.Password!, false);

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

		var result = new AuthenticateResult(
			accessToken,
			(int)TokenConsts.AccessTokenExpiration.TotalSeconds,
			refreshToken.Token,
			(int)TokenConsts.RefreshTokenExpiration.TotalSeconds
		);
		return Results.Ok(result);
	}
}

public class AuthenticateValidator : AbstractValidator<AuthenticateRequest>
{
	public AuthenticateValidator()
	{
		RuleFor(x => x.UsernameOrEmailAddress).NotEmpty().WithMessage("Please enter the username or email address");
		RuleFor(x => x.Password).NotEmpty().WithMessage("Please enter the password");
	}
}

public record AuthenticateRequest(
	string? UsernameOrEmailAddress,
	string? Password
);

public record AuthenticateResult(string AccessToken, int ExpireInSeconds, string RefreshToken, int RefreshTokenExpireInSeconds);
