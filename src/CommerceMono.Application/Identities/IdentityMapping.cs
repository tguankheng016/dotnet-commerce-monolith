using CommerceMono.Application.Identities.Dtos;
using CommerceMono.Application.Users.Models;
using Riok.Mapperly.Abstractions;

namespace CommerceMono.Application.Identities;

[Mapper]
public partial class IdentityMapper
{
#pragma warning disable RMG020 // Source member is not mapped to any target member
    public partial UserLoginInfoDto UserToUserLoginInfoDto(User user);
#pragma warning restore RMG020 // Source member is not mapped to any target member
}
