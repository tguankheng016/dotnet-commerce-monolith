using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CommerceMono.Modules.Core.Sessions;

public interface IAppSession
{
	long? UserId { get; }
}

public class AppSession : IAppSession
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public AppSession(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public long? UserId
	{
		get
		{
			var nameIdentifier = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

			if (!long.TryParse(nameIdentifier, out var userId))
			{
				return null;
			}

			return userId;
		}
	}
}
