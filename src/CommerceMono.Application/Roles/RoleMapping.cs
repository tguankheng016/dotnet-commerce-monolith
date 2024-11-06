using CommerceMono.Application.Roles.Dtos;
using CommerceMono.Application.Roles.Features.CreatingRole.V1;
using CommerceMono.Application.Roles.Features.UpdatingRole.V1;
using CommerceMono.Application.Roles.Models;
using Riok.Mapperly.Abstractions;

namespace CommerceMono.Application.Roles;

[Mapper]
public partial class RoleMapper
{
	public partial RoleDto RoleToRoleDto(Role role);

	public partial Role CreateRoleDtoToRole(CreateRoleDto createRoleDto);

	public partial void EditRoleDtoToRole(EditRoleDto editRoleDto, Role role);

	public partial CreateOrEditRoleDto RoleToCreateOrEditRoleDto(Role role);

	public partial CreateRoleCommand CreateRoleDtoToCreateRoleCommand(CreateRoleDto createRoleDto);

	public partial UpdateRoleCommand EditRoleDtoToUpdateRoleCommand(EditRoleDto createRoleDto);
}
