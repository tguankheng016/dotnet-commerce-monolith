using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CommerceMono.Application.Data;
using CommerceMono.Application.Users.Models;
using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Core.Dependencies;
using CommerceMono.Modules.Security;
using CommerceMono.Modules.Security.Caching;
using EasyCaching.Core;

namespace CommerceMono.Application.Identities.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
	private readonly AppDbContext _appDbContext;
	private readonly TokenAuthConfiguration _tokenAuthConfiguration;
	private readonly IEasyCachingProvider _cacheProvider;

	public JwtTokenGenerator(
		AppDbContext appDbContext,
		TokenAuthConfiguration tokenAuthConfiguration,
		ICacheManager cacheManager
	)
	{
		_appDbContext = appDbContext;
		_tokenAuthConfiguration = tokenAuthConfiguration;
		_cacheProvider = cacheManager.GetCachingProvider();
	}

	public async Task<string> CreateAccessToken(ClaimsIdentity identity, User user, string? refreshTokenKey = null, TimeSpan? expiration = null)
	{
		var claims = await CreateJwtClaims(identity, user, refreshTokenKey: refreshTokenKey);
		return CreateToken(claims, expiration ?? _tokenAuthConfiguration.AccessTokenExpiration);
	}

	public async Task<(string Token, string Key)> CreateRefreshToken(ClaimsIdentity identity, User user, TimeSpan? expiration = null)
	{
		var claims = (await CreateJwtClaims(identity, user, tokenType: AppTokenType.RefreshToken)).ToList();
		return (CreateToken(claims, expiration ?? _tokenAuthConfiguration.RefreshTokenExpiration),
			claims.First(c => c.Type == TokenConsts.TokenValidityKey).Value);
	}

	private string CreateToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
	{
		var now = DateTime.UtcNow;

		var jwtSecurityToken = new JwtSecurityToken(
			issuer: _tokenAuthConfiguration.Issuer,
			audience: _tokenAuthConfiguration.Audience,
			claims: claims,
			notBefore: now,
			signingCredentials: _tokenAuthConfiguration.SigningCredentials,
			expires: expiration == null ? null : now.Add(expiration.Value)
		);

		return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
	}

	private async Task<IEnumerable<Claim>> CreateJwtClaims(
		ClaimsIdentity identity,
		User user,
		TimeSpan? expiration = null,
		AppTokenType tokenType = AppTokenType.AccessToken,
		string? refreshTokenKey = null)
	{
		var tokenValidityKey = Guid.NewGuid().ToString();
		var claims = identity.Claims.ToList();

		claims.AddRange(new[]
		{
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
				ClaimValueTypes.Integer64),
			new Claim(TokenConsts.TokenValidityKey, tokenValidityKey),
			new Claim(TokenConsts.TokenType, ((int)tokenType).ToString())
		});

		if (!string.IsNullOrEmpty(refreshTokenKey))
		{
			claims.Add(new Claim(TokenConsts.RefreshTokenValidityKey, refreshTokenKey));
		}

		if (!expiration.HasValue)
		{
			expiration = tokenType == AppTokenType.AccessToken
				? TokenConsts.AccessTokenExpiration
				: TokenConsts.RefreshTokenExpiration;
		}

		var expirationDate = DateTimeOffset.Now.Add(expiration.Value);

		await _cacheProvider.SetAsync(
			TokenKeyCacheItem.GenerateCacheKey(user.Id, tokenValidityKey),
			tokenValidityKey,
			TimeSpan.FromHours(1)
		);

		await _cacheProvider.SetAsync(
			SecurityStampCacheItem.GenerateCacheKey(user.Id.ToString()),
			user.SecurityStamp,
			TimeSpan.FromHours(1)
		);

		await _appDbContext.UserTokens.AddAsync(new UserToken()
		{
			UserId = user.Id,
			LoginProvider = TokenConsts.LoginProvider,
			Name = tokenValidityKey,
			ExpireDate = expirationDate
		});

		await _appDbContext.SaveChangesAsync();

		return claims;
	}
}

public interface IJwtTokenGenerator : IScopedDependency
{
	Task<string> CreateAccessToken(ClaimsIdentity identity, User user, string? refreshTokenKey = null, TimeSpan? expiration = null);

	Task<(string Token, string Key)> CreateRefreshToken(ClaimsIdentity identity, User user, TimeSpan? expiration = null);
}
