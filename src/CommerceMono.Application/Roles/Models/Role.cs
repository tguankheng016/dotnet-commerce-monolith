using CommerceMono.Modules.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace CommerceMono.Application.Roles.Models;

public class Role : IdentityRole<long>, IFullAuditedEntity<long>
{
	public Role()
	{
	}

	public Role(string name)
	{
		Name = name;
	}

	public virtual bool IsStatic { get; set; }

	public virtual bool IsDefault { get; set; }

	public virtual long Version { get; set; }

	public virtual long? CreatorUserId { get; set; }

	public virtual DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;

	public virtual long? LastModifierUserId { get; set; }

	public virtual DateTimeOffset? LastModificationTime { get; set; }

	public virtual long? DeleterUserId { get; set; }

	public virtual DateTimeOffset? DeletionTime { get; set; }

	public virtual bool IsDeleted { get; set; }
}