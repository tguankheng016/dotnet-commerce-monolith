using CommerceMono.Modules.Core.Domain;

namespace CommerceMono.Application.Roles.Dtos;

public class CreateOrEditRoleDto : EntityDto<long?>
{
	public string Name { get; set; }

	public bool IsDefault { get; set; }

	public IList<string> GrantedPermissions { get; set; }

	public CreateOrEditRoleDto()
	{
		Name = "";
		GrantedPermissions = new List<string>();
	}
}

public class CreateRoleDto : CreateOrEditRoleDto;

public class EditRoleDto : CreateOrEditRoleDto;