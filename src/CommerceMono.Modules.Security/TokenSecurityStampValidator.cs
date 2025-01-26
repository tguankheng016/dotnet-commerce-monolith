using System.Security.Claims;
using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Security.Caching;
using EasyCaching.Core;

namespace CommerceMono.Modules.Security;

public class TokenSecurityStampValidator : ITokenSecurityStampValidator
{
	private readonly IEasyCachingProvider _cacheProvider;
	private readonly ITokenSecurityStampDbValidator _tokenSecurityStampDbValidator;

	public TokenSecurityStampValidator(
		ICacheManager cacheManager,
		ITokenSecurityStampDbValidator tokenSecurityStampDbValidator)
	{
		_tokenSecurityStampDbValidator = tokenSecurityStampDbValidator;
		_cacheProvider = cacheManager.GetCachingProvider();
	}

	public async Task<bool> ValidateAsync(ClaimsPrincipal claimsPrincipal)
	{
		if (claimsPrincipal?.Claims is null || !claimsPrincipal.Claims.Any())
		{
			return false;
		}

		var securityStampKey = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == TokenConsts.SecurityStampKey);

		if (securityStampKey is null)
		{
			return false;
		}

		var sub = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);

		if (sub is null)
		{
			return false;
		}

		var isValid = await ValidateSecurityStampFromCacheAsync(sub.Value, securityStampKey.Value);

		if (!isValid)
		{
			isValid = await _tokenSecurityStampDbValidator
				.ValidateSecurityStampFromDbAsync(GenerateCacheKey(sub.Value), sub.Value, securityStampKey.Value);
		}

		return isValid;
	}

	private async Task<bool> ValidateSecurityStampFromCacheAsync(string userId, string securityStamp)
	{
		var securityStampKey = await _cacheProvider.GetAsync<string>(GenerateCacheKey(userId));

		return securityStampKey is not null && securityStampKey.Value == securityStamp;
	}

	private string GenerateCacheKey(string userId) => SecurityStampCacheItem.GenerateCacheKey(userId);
}

public interface ITokenSecurityStampValidator
{
	Task<bool> ValidateAsync(ClaimsPrincipal claimsPrincipal);
}

public interface ITokenSecurityStampDbValidator
{
	Task<bool> ValidateSecurityStampFromDbAsync(string cacheKey, string userId, string securityStamp);
}