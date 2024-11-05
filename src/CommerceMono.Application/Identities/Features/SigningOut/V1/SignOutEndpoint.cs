using System.Security.Claims;
using CommerceMono.Application.Data;
using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Core.CQRS;
using CommerceMono.Modules.Core.EFCore;
using CommerceMono.Modules.Core.Sessions;
using CommerceMono.Modules.Security;
using CommerceMono.Modules.Security.Caching;
using CommerceMono.Modules.Web;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CommerceMono.Application.Identities.Features.SigningOut.V1;

public class SignOutEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapPost($"{EndpointConfig.BaseApiPath}/identities/sign-out", Handle)
			.WithName("SignOut")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("SignOut")
			.WithDescription("SignOut")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		IMediator mediator, ClaimsPrincipal User, CancellationToken cancellationToken
	)
	{
		if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
		{
			await mediator.Send(new SignOutCommand(User), cancellationToken);
		}

		return Results.Ok();
	}
}

// Results
public record SignOutResult();

// Command
public record SignOutCommand(ClaimsPrincipal UserClaim) : ICommand<SignOutResult>, ITransactional
{
}

// Handler
internal class SignOutHandler(
	AppDbContext appDbContext,
	IAppSession appSession,
	ICacheManager cacheManager
) : ICommandHandler<SignOutCommand, SignOutResult>
{
	public async Task<SignOutResult> Handle(SignOutCommand command, CancellationToken cancellationToken)
	{
		var userId = appSession.UserId;

		if (!userId.HasValue)
		{
			return new SignOutResult();
		}

		var tokenValidityKeyInClaims = command.UserClaim
			.Claims.First(c => c.Type == TokenConsts.TokenValidityKey);

		await RemoveTokenAsync(userId.Value, tokenValidityKeyInClaims.Value);

		var refreshTokenValidityKeyInClaims = command.UserClaim.Claims
			.FirstOrDefault(c => c.Type == TokenConsts.RefreshTokenValidityKey);

		if (refreshTokenValidityKeyInClaims != null)
		{
			await RemoveTokenAsync(userId.Value, refreshTokenValidityKeyInClaims.Value);
		}

		return new SignOutResult();
	}

	private async Task RemoveTokenAsync(long userId, string tokenKey)
	{
		var userToken = await appDbContext.UserTokens
			.FirstOrDefaultAsync(x => x.UserId == userId && x.Name == tokenKey);

		if (userToken != null)
		{
			// Remove Cache
			var _cacheProvider = cacheManager.GetCachingProvider();
			await _cacheProvider.RemoveAsync(TokenKeyCacheItem.GenerateCacheKey(userId, tokenKey));

			appDbContext.Remove(userToken);
			await appDbContext.SaveChangesAsync();
		}
	}
}
