namespace CommerceMono.Modules.Security;

public static class TokenConsts
{
	public const string LoginProvider = "TokenValidityKeyProvider";
	public const string TokenValidityKey = "token_validity_key";
	public const string RefreshTokenValidityKey = "refresh_token_validity_key";
	public const string SecurityStampKey = "AspNet.Identity.SecurityStamp";
	public const string TokenType = "token_type";
	public static TimeSpan AccessTokenExpiration = TimeSpan.FromDays(1);
	public static TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(30);
}

public enum AppTokenType
{
	AccessToken,
	RefreshToken
}