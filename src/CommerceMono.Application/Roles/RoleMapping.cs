using System.Diagnostics.CodeAnalysis;
using CommerceMono.Application.Roles.Dtos;
using CommerceMono.Application.Roles.Models;
using Riok.Mapperly.Abstractions;

namespace CommerceMono.Application.Roles;

[Mapper]
public partial class RoleMapper
{
	[SuppressMessage("Mapper", "RMG020")]
	[SuppressMessage("Mapper", "RMG012:Source member was not found for target member", Justification = "<Pending>")]
	public partial RoleDto RoleToRoleDto(Role role);
}
