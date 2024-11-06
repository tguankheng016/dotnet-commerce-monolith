using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Permissions.Caching;
using EasyCaching.Core;

namespace CommerceMono.Modules.Permissions;

public class PermissionManager : IPermissionManager
{
	private readonly IEasyCachingProvider _cacheProvider;
	private readonly IPermissionDbManager _permissionDbManager;

	public PermissionManager(
		IPermissionDbManager permissionDbManager,
		ICacheManager cacheManager
	)
	{
		_permissionDbManager = permissionDbManager;
		_cacheProvider = cacheManager.GetCachingProvider();
	}

	public async Task<bool> IsGrantedAsync(long userId, string permissionName, CancellationToken cancellationToken = default)
	{
		var grantedPermissions = await GetGrantedPermissionsAsync(userId, cancellationToken);
		return grantedPermissions.ContainsKey(permissionName);
	}

	public async Task<Dictionary<string, string>> GetGrantedPermissionsAsync(long userId, CancellationToken cancellationToken = default)
	{
		var grantedPermissions = new Dictionary<string, string>();
		var userProhibitedPermissions = new Dictionary<string, string>();

		var userRoles = await _cacheProvider
			.GetAsync<UserRoleCacheItem>(UserRoleCacheItem.GenerateCacheKey(userId), cancellationToken);

		if (userRoles is null || !userRoles.HasValue)
		{
			return await GetGrantedPermissionsFromDbAsync(userId);
		}

		var userPermissions = await _cacheProvider
			.GetAsync<UserPermissionCacheItem>(UserPermissionCacheItem.GenerateCacheKey(userId), cancellationToken);

		if (userPermissions != null && userPermissions.HasValue)
		{
			foreach (var kvp in userPermissions.Value.Permissions)
			{
				grantedPermissions.TryAdd(kvp.Key, kvp.Value);
			}

			userProhibitedPermissions = userPermissions.Value.ProhibitedPermissions;
		}

		foreach (var roleId in userRoles.Value.RoleIds)
		{
			var rolePermissions = await _cacheProvider
				.GetAsync<RolePermissionCacheItem>(RolePermissionCacheItem.GenerateCacheKey(roleId), cancellationToken);

			if (rolePermissions is null || !rolePermissions.HasValue)
			{
				return await GetGrantedPermissionsFromDbAsync(userId);
			}

			// Add those role permissions that is not prohibited at user level
			foreach (var kvp in rolePermissions.Value.Permissions.Where(item => !userProhibitedPermissions.ContainsKey(item.Key)))
			{
				grantedPermissions.TryAdd(kvp.Key, kvp.Value);
			}
		}

		return grantedPermissions;
	}

	private async Task<Dictionary<string, string>> GetGrantedPermissionsFromDbAsync(long userId)
	{
		return await _permissionDbManager.GetGrantedPermissionsAsync(userId);
	}
}

public interface IPermissionManager
{
	Task<Dictionary<string, string>> GetGrantedPermissionsAsync(long userId, CancellationToken cancellationToken = default);

	Task<bool> IsGrantedAsync(long userId, string permissionName, CancellationToken cancellationToken = default);
}

public interface IPermissionDbManager
{
	Task<Dictionary<string, string>> GetGrantedPermissionsAsync(long userId, CancellationToken cancellationToken = default);
}