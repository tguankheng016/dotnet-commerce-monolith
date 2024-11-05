using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceMono.Modules.Core.Domain;
using Microsoft.AspNetCore.Identity;

namespace CommerceMono.Application.Users.Models;

public class User : IdentityUser<long>, IFullAuditedEntity<long>
{
	public const int MaxFirstNameLength = 64;

	public const int MaxLastNameLength = 64;

	[Required]
	[StringLength(MaxFirstNameLength)]
	public virtual required string FirstName { get; set; }

	[Required]
	[StringLength(MaxLastNameLength)]
	public virtual required string LastName { get; set; }

	public virtual Guid ExternalUserId { get; set; }

	[NotMapped]
	public virtual string FullName => FirstName + " " + LastName;

	public virtual long Version { get; set; }

	public virtual long? CreatorUserId { get; set; }

	public virtual DateTimeOffset CreationTime { get; set; } = DateTimeOffset.Now;

	public virtual long? LastModifierUserId { get; set; }

	public virtual DateTimeOffset? LastModificationTime { get; set; }

	public virtual long? DeleterUserId { get; set; }

	public virtual DateTimeOffset? DeletionTime { get; set; }

	public virtual bool IsDeleted { get; set; }
}
