namespace CommerceMono.Modules.Security.Caching;

[Serializable]
public class SecurityStampCacheItem
{
    public static string GenerateCacheKey(string userId)
    {
        return $"{TokenConsts.SecurityStampKey}.{userId}";
    }
}
