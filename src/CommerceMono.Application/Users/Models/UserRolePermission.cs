
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceMono.Application.Roles.Models;
using CommerceMono.Modules.Core.Domain;

namespace CommerceMono.Application.Users.Models;

public class UserRolePermission : AuditedEntity
{
	public const int MaxPermissionNameLength = 128;

	[Required]
	[StringLength(MaxPermissionNameLength)]
	public virtual required string Name { get; set; }

	public virtual long? UserId { get; set; }

	public virtual long? RoleId { get; set; }

	public virtual bool IsGranted { get; set; }

	[ForeignKey(nameof(UserId))]
	public virtual User? UserFK { get; set; }

	[ForeignKey(nameof(RoleId))]
	public virtual Role? RoleFK { get; set; }
}
