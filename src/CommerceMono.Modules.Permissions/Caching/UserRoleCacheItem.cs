namespace CommerceMono.Modules.Permissions.Caching;

[Serializable]
public class UserRoleCacheItem
{
	public const string CacheName = "UserRoles";

	public long UserId { get; set; }

	public List<long> RoleIds { get; set; }

	public UserRoleCacheItem()
	{
		RoleIds = new List<long>();
	}

	public UserRoleCacheItem(long userId, List<long> roleIds)
	{
		UserId = userId;
		RoleIds = roleIds;
	}

	public static string GenerateCacheKey(long userId)
	{
		return $"{CacheName}:u{userId}";
	}
}
