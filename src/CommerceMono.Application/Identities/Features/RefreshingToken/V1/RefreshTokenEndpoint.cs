using System.Security.Claims;
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
using Microsoft.IdentityModel.Tokens;

namespace CommerceMono.Application.Identities.Features.RefreshingToken.V1;

public class RefreshTokenEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapPost($"{EndpointConfig.BaseApiPath}/identities/refresh-token", Handle)
			.WithName("RefreshToken")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.Produces<RefreshTokenResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("Refresh Token")
			.WithDescription("Refresh Token")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator, CancellationToken cancellationToken,
		[FromBody] RefreshTokenRequest request
	)
	{
		var command = new RefreshTokenCommand(request?.Token);
		var result = await mediator.Send(command, cancellationToken);

		return Results.Ok(result);
	}
}

// Request
public record RefreshTokenRequest(
	string? Token
);

// Result
public record RefreshTokenResult(string AccessToken, int ExpireInSeconds);

// Command
public record RefreshTokenCommand(string? Token) : ICommand<RefreshTokenResult>
{
}

// Validators
public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
	public RefreshTokenValidator()
	{
		RuleFor(x => x.Token).NotEmpty().WithMessage("Refresh token cannot be empty!");
	}
}

// Handler
internal class RefreshTokenHandler(
	UserManager<User> userManager,
	TokenAuthConfiguration tokenAuthConfiguration,
	IRefreshSecurityTokenHandler refreshSecurityTokenHandler,
	IJwtTokenGenerator jwtTokenGenerator,
	IUserClaimsPrincipalFactory<User> userClaimsPrincipal
) : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
{
	public async Task<RefreshTokenResult> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
	{
		var validationParameters = new TokenValidationParameters
		{
			ValidAudience = tokenAuthConfiguration.Audience,
			ValidIssuer = tokenAuthConfiguration.Issuer,
			IssuerSigningKey = tokenAuthConfiguration.SecurityKey
		};

		ClaimsPrincipal? principal = null;

		var sessionExpiredErrorMsg = $"Your session is expired!";

		try
		{
			principal = await refreshSecurityTokenHandler.ValidateRefreshToken(command.Token!, validationParameters);
		}
		catch (SecurityTokenException)
		{
			throw new BadRequestException(sessionExpiredErrorMsg);
		}

		if (principal == null)
		{
			throw new BadRequestException(sessionExpiredErrorMsg);
		}

		var refreshTokenKey = principal.Claims.FirstOrDefault(x => x.Type == TokenConsts.TokenValidityKey);

		var user = await userManager
			.FindByIdAsync(
				principal.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value
			);

		if (user == null)
		{
			throw new BadRequestException("Unknown user or user identifier");
		}

		principal = await userClaimsPrincipal.CreateAsync(user);
		var claimIdentity = principal.Identity as ClaimsIdentity;

		var accessToken = await jwtTokenGenerator
			.CreateAccessToken(claimIdentity!, user, refreshTokenKey: refreshTokenKey?.Value);

		return new RefreshTokenResult(
			accessToken,
			(int)TokenConsts.AccessTokenExpiration.TotalSeconds
		);
	}
}
