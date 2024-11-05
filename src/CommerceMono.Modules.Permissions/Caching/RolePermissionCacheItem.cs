namespace CommerceMono.Modules.Permissions.Caching;

[Serializable]
public class RolePermissionCacheItem
{
	public const string CacheName = "RolePermissions";

	public long RoleId { get; set; }

	public Dictionary<string, string> Permissions { get; set; }

	public RolePermissionCacheItem()
	{
		Permissions = new Dictionary<string, string>();
	}

	public RolePermissionCacheItem(long roleId, Dictionary<string, string> permissions)
	{
		RoleId = roleId;
		Permissions = permissions;
	}

	public static string GenerateCacheKey(long roleId)
	{
		return $"{CacheName}:r{roleId}";
	}
}