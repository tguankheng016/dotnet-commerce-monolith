using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace CommerceMono.Modules.Security;

public class RefreshSecurityTokenHandler : IRefreshSecurityTokenHandler
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly ITokenSecurityStampValidator _tokenSecurityStampValidator;
    private readonly ITokenKeyValidator _tokenKeyValidator;

    public RefreshSecurityTokenHandler(
        ITokenSecurityStampValidator tokenSecurityStampValidator,
        ITokenKeyValidator tokenKeyValidator
    )
    {
        _tokenSecurityStampValidator = tokenSecurityStampValidator;
        _tokenKeyValidator = tokenKeyValidator;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public async Task<ClaimsPrincipal> ValidateRefreshToken(string securityToken, TokenValidationParameters validationParameters)
    {
        var principal = _tokenHandler.ValidateToken(securityToken, validationParameters, out var validatedToken);

        var tokenTypeValue = principal.Claims.FirstOrDefault(x => x.Type == TokenConsts.TokenType)?.Value;

        if (tokenTypeValue != ((int)AppTokenType.RefreshToken).ToString())
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

        return principal;
    }
}

public interface IRefreshSecurityTokenHandler
{
    Task<ClaimsPrincipal> ValidateRefreshToken(string securityToken,
        TokenValidationParameters validationParameters);
}
