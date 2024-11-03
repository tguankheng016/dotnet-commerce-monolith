using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CommerceMono.Modules.Security;

public class AccessSecurityTokenHandler : JwtSecurityTokenHandler
{
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly IServiceScopeFactory serviceScopeFactory;

    public AccessSecurityTokenHandler(
        IServiceScopeFactory serviceScopeFactory
    )
    {
        this.serviceScopeFactory = serviceScopeFactory;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public override ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters,
        out SecurityToken validatedToken)
    {
        var principal = _tokenHandler.ValidateToken(securityToken, validationParameters, out validatedToken);

        if (!HasTokenType(principal, AppTokenType.AccessToken))
        {
            throw new SecurityTokenException("invalid token type");
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        var tokenSecurityStampValidator = scope.ServiceProvider.GetRequiredService<ITokenSecurityStampValidator>();

        if (!Task.Run(() => tokenSecurityStampValidator.ValidateAsync(principal)).GetAwaiter().GetResult())
        {
            throw new SecurityTokenException("invalid");
        }

        var tokenKeyValidator = scope.ServiceProvider.GetRequiredService<ITokenKeyValidator>();

        if (!Task.Run(() => tokenKeyValidator.ValidateAsync(principal)).GetAwaiter().GetResult())
        {
            throw new SecurityTokenException("invalid");
        }

        return principal;
    }

    private bool HasTokenType(ClaimsPrincipal principal, AppTokenType tokenType)
    {
        return principal.Claims
            .FirstOrDefault(x => x.Type == TokenConsts.TokenType)?.Value == ((int)tokenType).ToString();
    }
}
