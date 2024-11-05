using CommerceMono.Modules.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace CommerceMono.Application.Users.Models;

public class UserRole : IdentityUserRole<long>, IAuditedEntity<long>
{
	public virtual long Id { get; set; }

	public virtual long Version { get; set; }

	public virtual long? CreatorUserId { get; set; }

	public virtual DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;

	public virtual long? LastModifierUserId { get; set; }

	public virtual DateTimeOffset? LastModificationTime { get; set; }
}
