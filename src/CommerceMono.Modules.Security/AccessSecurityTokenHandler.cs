using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace CommerceMono.Modules.Security;

public class AccessSecurityTokenHandler
{
	private readonly ITokenSecurityStampValidator _tokenSecurityStampValidator;
	private readonly ITokenKeyValidator _tokenKeyValidator;

	public AccessSecurityTokenHandler(
		ITokenSecurityStampValidator tokenSecurityStampValidator,
		ITokenKeyValidator tokenKeyValidator)
	{
		_tokenSecurityStampValidator = tokenSecurityStampValidator;
		_tokenKeyValidator = tokenKeyValidator;
	}

	public async Task<bool> ValidateTokenAsync(ClaimsPrincipal principal)
	{
		if (!HasTokenType(principal, AppTokenType.AccessToken))
		{
			throw new SecurityTokenException("invalid token type");
		}

		if (!await _tokenSecurityStampValidator.ValidateAsync(principal))
		{
			throw new SecurityTokenException("invalid");
		}

		if (!await _tokenKeyValidator.ValidateAsync(principal))
		{
			throw new SecurityTokenException("invalid");
		}

		return true;
	}

	private bool HasTokenType(ClaimsPrincipal principal, AppTokenType tokenType)
	{
		return principal.Claims
			.FirstOrDefault(x => x.Type == TokenConsts.TokenType)?.Value == ((int)tokenType).ToString();
	}
}