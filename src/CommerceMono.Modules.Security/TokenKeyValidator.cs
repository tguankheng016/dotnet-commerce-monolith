using System.Security.Claims;
using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Security.Caching;
using EasyCaching.Core;

namespace CommerceMono.Modules.Security;

public class TokenKeyValidator : ITokenKeyValidator
{
	private readonly ITokenKeyDbValidator _tokenKeyDbValidator;
	private readonly IEasyCachingProvider _cacheProvider;

	public TokenKeyValidator(
		ICacheManager cacheManager,
		ITokenKeyDbValidator tokenKeyDbValidator)
	{
		_tokenKeyDbValidator = tokenKeyDbValidator;
		_cacheProvider = cacheManager.GetCachingProvider();
	}

	public async Task<bool> ValidateAsync(ClaimsPrincipal claimsPrincipal)
	{
		if (claimsPrincipal?.Claims == null || !claimsPrincipal.Claims.Any())
		{
			return false;
		}

		var tokenKey = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == TokenConsts.TokenValidityKey);

		if (tokenKey == null)
		{
			return false;
		}

		var sub = claimsPrincipal.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);

		if (sub == null)
		{
			return false;
		}

		long.TryParse(sub.Value, out long userId);

		var isValid = await ValidateTokenKeyFromCacheAsync(userId, tokenKey.Value);

		if (!isValid)
		{
			isValid = await _tokenKeyDbValidator.ValidateTokenKeyFromDbAsync(GenerateCacheKey(userId, tokenKey.Value), userId, tokenKey.Value);
		}

		return isValid;
	}

	private async Task<bool> ValidateTokenKeyFromCacheAsync(long userId, string tokenKey)
	{
		var tokenKeyCache = await _cacheProvider.GetAsync<string>(GenerateCacheKey(userId, tokenKey));

		return tokenKeyCache != null && tokenKeyCache.HasValue;
	}

	private string GenerateCacheKey(long userId, string tokenKey) =>
		TokenKeyCacheItem.GenerateCacheKey(userId, tokenKey);
}

public interface ITokenKeyValidator
{
	Task<bool> ValidateAsync(ClaimsPrincipal claimsPrincipal);
}

public interface ITokenKeyDbValidator
{
	Task<bool> ValidateTokenKeyFromDbAsync(string cacheKey, long userId, string tokenKey);
}