using CommerceMono.Modules.Core.Domain;

namespace CommerceMono.Application.Roles.Dtos;

public class RoleDto : AuditedEntityDto<long>
{
    public required string Name { get; set; }

    public bool IsStatic { get; set; }

    public bool IsDefault { get; set; }

    public bool IsAssigned { get; set; }
}
