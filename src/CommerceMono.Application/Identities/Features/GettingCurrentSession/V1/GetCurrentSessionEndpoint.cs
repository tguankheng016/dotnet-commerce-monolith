using CommerceMono.Application.Identities.Dtos;
using CommerceMono.Application.Users.Models;
using CommerceMono.Modules.Core.Sessions;
using CommerceMono.Modules.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;

namespace CommerceMono.Application.Identities.Features.GettingCurrentSession.V1;

public class GetCurrentSessionEndpoint : IMinimalEndpoint
{
	public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
	{
		builder.MapGet($"{EndpointConfig.BaseApiPath}/identities/current-session", Handle)
			.WithName("GetCurrentSession")
			.WithApiVersionSet(builder.GetApiVersionSet())
			.Produces<GetCurrentSessionResult>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.WithSummary("GetCurrentSession")
			.WithDescription("GetCurrentSession")
			.WithOpenApi()
			.HasApiVersion(1.0);

		return builder;
	}

	async Task<IResult> Handle(
		UserManager<User> userManager,
		IAppSession appSession,
		CancellationToken cancellationToken
	)
	{
		var userId = appSession.UserId;

		UserLoginInfoDto userDto = null;

		if (userId.HasValue)
		{
			var user = await userManager.FindByIdAsync(userId.Value.ToString());

			if (user != null)
			{
				var mapper = new IdentityMapper();
				userDto = mapper.UserToUserLoginInfoDto(user);

			}
		}

		var result = new GetCurrentSessionResult(userDto, new Dictionary<string, bool>(), new Dictionary<string, bool>());

		return Results.Ok(result);
	}
}

public record GetCurrentSessionResult(UserLoginInfoDto User, Dictionary<string, bool> AllPermissions, Dictionary<string, bool> GrantedPermissions);
