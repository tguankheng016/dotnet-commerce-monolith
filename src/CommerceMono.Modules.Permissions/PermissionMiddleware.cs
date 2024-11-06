using System.Security.Claims;
using CommerceMono.Modules.Core.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CommerceMono.Modules.Permissions;

public class PermissionMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<PermissionMiddleware> _logger;

	public PermissionMiddleware(
		RequestDelegate next,
		ILogger<PermissionMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(
		HttpContext context,
		IAppSession appSession,
		IPermissionManager permissionManager
	)
	{
		var userId = appSession.UserId;

		// 1 - if the request is not authenticated, nothing to do
		if (context.User.Identity is null || !context.User.Identity.IsAuthenticated || !userId.HasValue)
		{
			await _next(context);

			return;
		}

		var endpoint = context.GetEndpoint();
		var requiresAuthorization = false;

		if (endpoint is not null)
		{
			var metadata = endpoint.Metadata;
			requiresAuthorization = metadata.OfType<AuthorizeAttribute>().Any();
		}

		if (requiresAuthorization)
		{
			var cancellationToken = context.RequestAborted;

			var grantedPermissions = await permissionManager.GetGrantedPermissionsAsync(userId.Value, cancellationToken);

			var permissionClaims = grantedPermissions.Select(x => new Claim(PermissionConsts.PermissionClaimName, x.Key));

			var permissionsIdentity = new ClaimsIdentity(nameof(PermissionMiddleware), "name", "role");
			permissionsIdentity.AddClaims(permissionClaims);

			context.User.AddIdentity(permissionsIdentity);
		}

		await _next(context);
	}
}
