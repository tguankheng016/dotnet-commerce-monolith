using CommerceMono.Modules.Core.Domain;

namespace CommerceMono.Application.Users.Dtos;

public class CreateOrEditUserDto : EntityDto<long?>
{
    public string? UserName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? ConfirmPassword { get; set; }

    public IList<string> Roles { get; set; } = new List<string>();
}

public class CreateUserDto : CreateOrEditUserDto;

public class EditUserDto : CreateOrEditUserDto;