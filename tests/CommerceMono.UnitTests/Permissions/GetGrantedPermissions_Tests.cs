using System.Collections;
using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Permissions;
using CommerceMono.Modules.Permissions.Caching;
using EasyCaching.Core;

namespace CommerceMono.UnitTests.Permissions;

public class GetGrantedPermissions_Tests
{
	private readonly IEasyCachingProvider _cacheProvider;
	private readonly ICacheManager _cacheManager;
	private readonly IPermissionDbManager _permissionDbManager;

	public GetGrantedPermissions_Tests()
	{
		_cacheProvider = Substitute.For<IEasyCachingProvider>();
		_cacheManager = Substitute.For<ICacheManager>();
		_permissionDbManager = Substitute.For<IPermissionDbManager>();
	}

	[Theory]
	[ClassData(typeof(GetCorrectGrantedPermissionsTestData))]
	public async Task Should_Get_Correct_Granted_Permissions(long userId, Dictionary<string, string> expectedGrantedPermissions)
	{
		// Arrange
		PrepareMocks(userId);

		_cacheManager.GetCachingProvider().Returns(_cacheProvider);

		var permissionManager = new PermissionManager(_permissionDbManager, _cacheManager);

		// Act
		var grantedPermissions = await permissionManager.GetGrantedPermissionsAsync(userId);
		var isGranted = await permissionManager.IsGrantedAsync(userId, expectedGrantedPermissions.First().Key);

		// Assert
		grantedPermissions.Should().Equal(expectedGrantedPermissions);
		isGranted.Should().BeTrue();
	}

	private void PrepareMocks(long userId)
	{
		switch (userId)
		{
			case 1:
				{
					// No role but have granted some permissions
					// Have caches
					var userRoleCacheItem = new UserRoleCacheItem();

					_cacheProvider.GetAsync<UserRoleCacheItem>(Arg.Any<string>(), default)
						.Returns(async (callInfo) =>
							await Task.FromResult(new CacheValue<UserRoleCacheItem>(userRoleCacheItem, true))
						);

					var grantedPermissions = new Dictionary<string, string>()
					{
						{ UserPermissions.Pages_Administration_Users, "" },
						{ RolePermissions.Pages_Administration_Roles, "" }
					};

					var userPermissionCacheItem = new UserPermissionCacheItem(userId, grantedPermissions, new Dictionary<string, string>());

					_cacheProvider.GetAsync<UserPermissionCacheItem>(Arg.Any<string>(), default)
						.Returns(async (callInfo) =>
							await Task.FromResult(new CacheValue<UserPermissionCacheItem>(userPermissionCacheItem, true))
						);
					break;
				}
			case 2:
				{
					// Have role and role permissions
					// No user permissions
					// Have caches
					var roleId = 1;
					var roleIds = new List<long>() { roleId };
					var userRoleCacheItem = new UserRoleCacheItem(userId, roleIds);

					_cacheProvider.GetAsync<UserRoleCacheItem>(Arg.Any<string>(), default)
						.Returns(async (callInfo) =>
							await Task.FromResult(new CacheValue<UserRoleCacheItem>(userRoleCacheItem, true))
						);

					var grantedPermissions = new Dictionary<string, string>()
					{
						{ UserPermissions.Pages_Administration_Users, "" }
					};

					var rolePermissionCacheItem = new RolePermissionCacheItem(roleId, grantedPermissions);

					_cacheProvider.GetAsync<RolePermissionCacheItem>(Arg.Any<string>(), default)
						.Returns(async (callInfo) =>
							await Task.FromResult(new CacheValue<RolePermissionCacheItem>(rolePermissionCacheItem, true))
						);

					var userPermissionCacheItem = new UserPermissionCacheItem(userId, new Dictionary<string, string>(), new Dictionary<string, string>());

					_cacheProvider.GetAsync<UserPermissionCacheItem>(Arg.Any<string>(), default)
						.Returns(async (callInfo) =>
							await Task.FromResult(new CacheValue<UserPermissionCacheItem>(userPermissionCacheItem, true))
						);

					break;
				}
			case 3:
				{
					// Have role and role permissions and user permissions
					// Have prohibited user permissions
					// Have caches
					var roleId = 1;
					var roleIds = new List<long>() { roleId };
					var userRoleCacheItem = new UserRoleCacheItem(userId, roleIds);

					_cacheProvider.GetAsync<UserRoleCacheItem>(Arg.Any<string>(), default)
						.Returns(async (callInfo) =>
							await Task.FromResult(new CacheValue<UserRoleCacheItem>(userRoleCacheItem, true))
						);

					var grantedPermissions = new Dictionary<string, string>()
					{
						{ UserPermissions.Pages_Administration_Users, "" },
						{ RolePermissions.Pages_Administration_Roles, "" }
					};

					var rolePermissionCacheItem = new RolePermissionCacheItem(roleId, grantedPermissions);

					_cacheProvider.GetAsync<RolePermissionCacheItem>(Arg.Any<string>(), default)
						.Returns(async (callInfo) =>
							await Task.FromResult(new CacheValue<RolePermissionCacheItem>(rolePermissionCacheItem, true))
						);

					var userProhibitedPermissions = new Dictionary<string, string>()
					{
						{ UserPermissions.Pages_Administration_Users, "" }
					};

					var userPermissionCacheItem = new UserPermissionCacheItem(userId, permissions: new Dictionary<string, string>(), prohibitedPermissions: userProhibitedPermissions);

					_cacheProvider.GetAsync<UserPermissionCacheItem>(Arg.Any<string>(), default)
						.Returns(async (callInfo) =>
							await Task.FromResult(new CacheValue<UserPermissionCacheItem>(userPermissionCacheItem, true))
						);

					break;
				}
			case 4:
				{
					// No caches

					var grantedPermissions = new Dictionary<string, string>()
					{
						{ UserPermissions.Pages_Administration_Users, "" }
					};

					_permissionDbManager.GetGrantedPermissionsAsync(Arg.Any<long>(), default)
						.Returns(Task.FromResult(grantedPermissions));
					break;
				}
		}
	}

	private class GetCorrectGrantedPermissionsTestData : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			// Scenario 1
			yield return new object[]
			{
				1,
				new Dictionary<string, string>()
				{
					{ UserPermissions.Pages_Administration_Users, "" },
					{ RolePermissions.Pages_Administration_Roles, "" }
				}
			};
			// Scenario 2
			yield return new object[]
			{
				2,
				new Dictionary<string, string>()
				{
					{ UserPermissions.Pages_Administration_Users, "" }
				}
			};
			// Scenario 3
			yield return new object[]
			{
				3,
				new Dictionary<string, string>()
				{
					{ RolePermissions.Pages_Administration_Roles, "" }
				}
			};
			// Scenario 4
			yield return new object[]
			{
				4,
				new Dictionary<string, string>()
				{
					{ UserPermissions.Pages_Administration_Users, "" }
				}
			};
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
