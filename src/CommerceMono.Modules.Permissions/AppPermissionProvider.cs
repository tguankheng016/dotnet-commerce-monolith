using System;

namespace CommerceMono.Modules.Permissions;

public static class UserPermissions
{
	public const string GroupName = "Users";

	public const string Pages_Administration_Users = "Pages.Administration.Users";
	public const string Pages_Administration_Users_Create = "Pages.Administration.Users.Create";
	public const string Pages_Administration_Users_Edit = "Pages.Administration.Users.Edit";
	public const string Pages_Administration_Users_Delete = "Pages.Administration.Users.Delete";
	public const string Pages_Administration_Users_ChangePermissions = "Pages.Administration.Users.ChangePermissions";
}

public static class RolePermissions
{
	public const string GroupName = "Roles";

	public const string Pages_Administration_Roles = "Pages.Administration.Roles";
	public const string Pages_Administration_Roles_Create = "Pages.Administration.Roles.Create";
	public const string Pages_Administration_Roles_Edit = "Pages.Administration.Roles.Edit";
	public const string Pages_Administration_Roles_Delete = "Pages.Administration.Roles.Delete";
}

public static class AppPermissionProvider
{
	public static List<Permission> GetPermissions()
	{
		var permissions = new List<Permission>
		{
			new Permission(RolePermissions.Pages_Administration_Roles, "View roles", RolePermissions.GroupName),
			new Permission(RolePermissions.Pages_Administration_Roles_Create, "Create role", RolePermissions.GroupName),
			new Permission(RolePermissions.Pages_Administration_Roles_Edit, "Edit role", RolePermissions.GroupName),
			new Permission(RolePermissions.Pages_Administration_Roles_Delete, "Delete role", RolePermissions.GroupName),

			new Permission(UserPermissions.Pages_Administration_Users, "View users", UserPermissions.GroupName),
			new Permission(UserPermissions.Pages_Administration_Users_Create, "Create user", UserPermissions.GroupName),
			new Permission(UserPermissions.Pages_Administration_Users_Edit, "Edit user", UserPermissions.GroupName),
			new Permission(UserPermissions.Pages_Administration_Users_Delete, "Delete user", UserPermissions.GroupName),
			new Permission(UserPermissions.Pages_Administration_Users_ChangePermissions, "Change user permissions", UserPermissions.GroupName),
		};

		return permissions;
	}
}