using CommerceMono.Application.Users.Services;
using CommerceMono.Modules.Permissions;

namespace CommerceMono.Application.Identities.Services;

public class PermissionDbManager : IPermissionDbManager
{
	private readonly IUserRolePermissionManager _userRolePermissionManager;

	public PermissionDbManager(
		IUserRolePermissionManager userRolePermissionManager
	)
	{
		_userRolePermissionManager = userRolePermissionManager;
	}

	public async Task<Dictionary<string, string>> GetGrantedPermissionsAsync(long userId, CancellationToken cancellationToken = default)
	{
		var grantedPermissions = await _userRolePermissionManager.SetUserPermissionAsync(userId, cancellationToken);

		return grantedPermissions;
	}
}
