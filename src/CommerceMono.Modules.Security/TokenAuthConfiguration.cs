using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CommerceMono.Modules.Security;

public class TokenAuthConfiguration
{
	public SymmetricSecurityKey SecurityKey { get; set; }

	public string Issuer { get; set; }

	public string Audience { get; set; }

	public SigningCredentials SigningCredentials { get; set; }

	public TimeSpan AccessTokenExpiration { get; set; }

	public TimeSpan RefreshTokenExpiration { get; set; }

	public TokenAuthConfiguration(IConfiguration configuration)
	{
		Issuer = configuration["Authentication:JwtBearer:Issuer"]!;
		Audience = configuration["Authentication:JwtBearer:Audience"]!;
		SecurityKey = new SymmetricSecurityKey(
			Encoding.ASCII.GetBytes(
				configuration["Authentication:JwtBearer:SecurityKey"]!
			)
		);
		SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
		AccessTokenExpiration = TokenConsts.AccessTokenExpiration;
		RefreshTokenExpiration = TokenConsts.RefreshTokenExpiration;
	}
}
