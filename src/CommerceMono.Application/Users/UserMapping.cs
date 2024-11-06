using CommerceMono.Application.Users.Dtos;
using CommerceMono.Application.Users.Features.CreatingUser.V1;
using CommerceMono.Application.Users.Features.UpdatingUser.V1;
using CommerceMono.Application.Users.Models;
using Riok.Mapperly.Abstractions;

namespace CommerceMono.Application.Users;

[Mapper]
public partial class UserMapper
{
	public partial UserDto UserToUserDto(User user);

	[MapperIgnoreSource(nameof(CreateUserDto.Password))]
	public partial User CreateUserDtoToUser(CreateUserDto createUserDto);

	public partial CreateUserCommand CreateUserDtoToCreateUserCommand(CreateUserDto createUserDto);

	public partial CreateOrEditUserDto UserToCreateOrEditUserDto(User user);

	public partial void EditUserDtoToUser(EditUserDto editUserDto, User user);

	public partial UpdateUserCommand EdiUserDtoToUpdateUserCommand(EditUserDto editUserDto);
}
