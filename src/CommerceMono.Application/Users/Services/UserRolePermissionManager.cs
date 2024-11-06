using CommerceMono.Application.Data;
using CommerceMono.Application.Roles.Constants;
using CommerceMono.Application.Roles.Models;
using CommerceMono.Application.Users.Models;
using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Core.Dependencies;
using CommerceMono.Modules.Core.Exceptions;
using CommerceMono.Modules.Permissions;
using CommerceMono.Modules.Permissions.Caching;
using EasyCaching.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CommerceMono.Application.Users.Services;

public class UserRolePermissionManager : IUserRolePermissionManager
{
	private readonly IEasyCachingProvider _cacheProvider;
	private readonly UserManager<User> _userManager;
	private readonly RoleManager<Role> _roleManager;
	private readonly AppDbContext _appDbContext;
	private readonly AppPermissions _appPermissions;

	public UserRolePermissionManager(
		ICacheManager cacheManager,
		UserManager<User> userManager,
		RoleManager<Role> roleManager,
		AppDbContext appDbContext,
		AppPermissions appPermissions)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_appDbContext = appDbContext;
		_appPermissions = appPermissions;
		_cacheProvider = cacheManager.GetCachingProvider();
	}

	public void ValidatePermissions(IList<string> permissions)
	{
		var validPermissionNames = _appPermissions.Items.Select(x => x.Name).ToList();
		var undefinedPermissions = permissions
			.Where(x => !validPermissionNames.Contains(x))
			.ToList();

		if (undefinedPermissions.Count > 0)
		{
			throw new BadRequestException($"There are {undefinedPermissions.Count} invalid permissions");
		}
	}

	public async Task<Dictionary<string, string>> SetUserPermissionAsync(long userId, CancellationToken cancellationToken = default)
	{
		var grantedPermissions = new Dictionary<string, string>();

		var user = await _userManager.FindByIdAsync(userId.ToString());

		if (user is null)
		{
			throw new BadRequestException("User not found");
		}

		var userRolesCaches = await _cacheProvider
			.GetAsync<UserRoleCacheItem>(UserRoleCacheItem.GenerateCacheKey(userId), cancellationToken);

		var roleIds = new List<long>();

		if (userRolesCaches is null || !userRolesCaches.HasValue)
		{
			var userRoles = await _userManager.GetRolesAsync(user);

			foreach (var userRole in userRoles)
			{
				var role = await _roleManager.FindByNameAsync(userRole);

				if (role != null)
				{
					roleIds.Add(role.Id);
				}
			}

			await _cacheProvider
				.SetAsync(
					UserRoleCacheItem.GenerateCacheKey(userId),
					new UserRoleCacheItem(userId, roleIds),
					TimeSpan.FromHours(1),
					cancellationToken
				);
		}
		else
		{
			roleIds = userRolesCaches.Value.RoleIds;
		}

		var userPermissions = await _appDbContext.UserRolePermissions
			.Where(x => x.UserId == userId)
			.ToListAsync(cancellationToken);

		var grantedUserPermissions = userPermissions.Where(x => x.IsGranted).ToDictionary(x => x.Name, x => x.Name);
		var prohibitedUserPermissions = userPermissions.Where(x => !x.IsGranted).ToDictionary(x => x.Name, x => x.Name);

		await _cacheProvider
			.SetAsync(
				UserPermissionCacheItem.GenerateCacheKey(userId),
				new UserPermissionCacheItem(userId, grantedUserPermissions, prohibitedUserPermissions),
				TimeSpan.FromHours(1),
				cancellationToken
			);

		foreach (var kvp in grantedUserPermissions)
		{
			grantedPermissions.TryAdd(kvp.Key, kvp.Value);
		}

		foreach (var roleId in roleIds)
		{
			var rolePermissions = await SetRolePermissionAsync(roleId, cancellationToken);

			foreach (var kvp in rolePermissions.Where(item => !prohibitedUserPermissions.ContainsKey(item.Key)))
			{
				grantedPermissions.TryAdd(kvp.Key, kvp.Value);
			}
		}

		return grantedPermissions;
	}

	public async Task<Dictionary<string, string>> SetRolePermissionAsync(long roleId, CancellationToken cancellationToken = default)
	{
		var role = await _roleManager.FindByIdAsync(roleId.ToString());

		if (role is null)
		{
			throw new BadRequestException("Role not found");
		}

		var isAdmin = role.NormalizedName == RoleConsts.RoleName.Admin.ToUpper();

		var rolePermissions = await _appDbContext.UserRolePermissions
			.Where(x => x.RoleId == roleId)
			.ToListAsync(cancellationToken);

		if (isAdmin)
		{
			var allPermissions = _appPermissions.Items.Select(x => x.Name).ToList();
			var prohibitedPermissions = rolePermissions.Where(x => !x.IsGranted).Select(x => x.Name).ToList();
			var grantedPermissions = allPermissions
				.Where(item => !prohibitedPermissions.Contains(item))
				.ToDictionary(x => x.ToString(), x => x.ToString());

			await _cacheProvider
				.SetAsync(
					RolePermissionCacheItem.GenerateCacheKey(roleId),
					new RolePermissionCacheItem(
						roleId,
						grantedPermissions
					),
					TimeSpan.FromHours(2),
					cancellationToken
				);

			return grantedPermissions;
		}
		else
		{
			var grantedPermissions = rolePermissions
				.Where(x => x.IsGranted).Select(x => x.Name)
				.ToDictionary(x => x.ToString(), x => x.ToString());

			await _cacheProvider
				.SetAsync(
					RolePermissionCacheItem.GenerateCacheKey(roleId),
					new RolePermissionCacheItem(
						roleId,
						grantedPermissions
					),
					TimeSpan.FromHours(2),
					cancellationToken
				);

			return grantedPermissions;
		}
	}

	public async Task RemoveUserRoleCacheAsync(long userId, CancellationToken cancellationToken = default)
	{
		await _cacheProvider.RemoveAsync(UserRoleCacheItem.GenerateCacheKey(userId), cancellationToken);
	}
}


public interface IUserRolePermissionManager : IScopedDependency
{
	void ValidatePermissions(IList<string> permissions);

	Task<Dictionary<string, string>> SetUserPermissionAsync(long userId, CancellationToken cancellationToken = default);

	Task<Dictionary<string, string>> SetRolePermissionAsync(long role, CancellationToken cancellationToken = default);

	Task RemoveUserRoleCacheAsync(long userId, CancellationToken cancellationToken = default);
}