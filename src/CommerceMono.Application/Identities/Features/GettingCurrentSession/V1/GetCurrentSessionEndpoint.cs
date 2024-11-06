using CommerceMono.Application.Identities.Dtos;
using CommerceMono.Application.Users.Models;
using CommerceMono.Modules.Core.CQRS;
using CommerceMono.Modules.Core.Sessions;
using CommerceMono.Modules.Permissions;
using CommerceMono.Modules.Web;
using MediatR;
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
		IMediator mediator, CancellationToken cancellationToken
	)
	{
		var query = new GetCurrentSessionQuery();
		var result = await mediator.Send(query, cancellationToken);

		return Results.Ok(result);
	}
}

// Result
public record GetCurrentSessionResult(UserLoginInfoDto? User, Dictionary<string, bool> AllPermissions, Dictionary<string, bool> GrantedPermissions);

// Query
public record GetCurrentSessionQuery() : IQuery<GetCurrentSessionResult>
{
}

// Handler
internal class GetCurrentSessionHandler(
	UserManager<User> userManager,
	IAppSession appSession,
	AppPermissions appPermissions,
	IPermissionManager permissionManager
) : IQueryHandler<GetCurrentSessionQuery, GetCurrentSessionResult>
{
	public async Task<GetCurrentSessionResult> Handle(GetCurrentSessionQuery request, CancellationToken cancellationToken)
	{
		var userId = appSession.UserId;

		UserLoginInfoDto? userDto = null;

		var allPermissions = appPermissions.Items.ToDictionary(p => p.Name, p => true);
		var grantedPermissions = new Dictionary<string, bool>();

		if (userId.HasValue)
		{
			var user = await userManager.FindByIdAsync(userId.Value.ToString());

			if (user is not null)
			{
				var mapper = new IdentityMapper();
				userDto = mapper.UserToUserLoginInfoDto(user);
				grantedPermissions = (await permissionManager.GetGrantedPermissionsAsync(user.Id, cancellationToken))
					.ToDictionary(x => x.Key, x => true);
			}
		}

		return new GetCurrentSessionResult(userDto, allPermissions, grantedPermissions);
	}
}