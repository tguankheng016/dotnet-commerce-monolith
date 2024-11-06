using CommerceMono.Application.Users.Dtos;
using CommerceMono.Application.Users.Features.CreatingUser.V1;
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
}
