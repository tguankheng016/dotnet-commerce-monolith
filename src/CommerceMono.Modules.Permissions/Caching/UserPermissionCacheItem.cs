namespace CommerceMono.Modules.Permissions.Caching;

[Serializable]
public class UserPermissionCacheItem
{
	public const string CacheName = "UserPermissions";

	public long UserId { get; set; }

	public Dictionary<string, string> Permissions { get; set; }

	public Dictionary<string, string> ProhibitedPermissions { get; set; }

	public UserPermissionCacheItem()
	{
		Permissions = new Dictionary<string, string>();
		ProhibitedPermissions = new Dictionary<string, string>();
	}

	public UserPermissionCacheItem(long userId, Dictionary<string, string> permissions, Dictionary<string, string> prohibitedPermissions)
	{
		UserId = userId;
		Permissions = permissions;
		ProhibitedPermissions = prohibitedPermissions;
	}

	public static string GenerateCacheKey(long userId)
	{
		return $"{CacheName}:u{userId}";
	}
}
