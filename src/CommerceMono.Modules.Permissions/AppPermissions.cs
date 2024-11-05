namespace CommerceMono.Modules.Permissions;

public class Permission
{
	public virtual string Name { get; set; }

	public virtual string DisplayName { get; set; }

	public virtual string Group { get; set; }

	public Permission(string name, string displayName, string group)
	{
		Name = name;
		DisplayName = displayName;
		Group = group;
	}
}

public sealed class AppPermissions
{
	public AppPermissions()
	{
		Items = new List<Permission>();
	}

	public IReadOnlyList<Permission> Items { get; set; }
}