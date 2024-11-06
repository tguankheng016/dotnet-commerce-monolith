using CommerceMono.Application.Users.Dtos;
using CommerceMono.Application.Users.Models;
using Riok.Mapperly.Abstractions;

namespace CommerceMono.Application.Users;

[Mapper]
public partial class UserMapper
{
	public partial UserDto UserToUserDto(User user);
}
