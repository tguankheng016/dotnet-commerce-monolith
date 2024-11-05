namespace CommerceMono.Modules.Security.Caching;

[Serializable]
public class TokenKeyCacheItem
{
    public static string GenerateCacheKey(long userId, string tokenKey)
    {
        return $"{TokenConsts.TokenValidityKey}.{userId}.{tokenKey}";
    }
}
