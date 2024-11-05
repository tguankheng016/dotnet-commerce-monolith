using System.Diagnostics.CodeAnalysis;
using CommerceMono.Application.Identities.Dtos;
using CommerceMono.Application.Identities.Features.Authenticating.V2;
using CommerceMono.Application.Users.Models;
using Riok.Mapperly.Abstractions;

namespace CommerceMono.Application.Identities;

[Mapper]
public partial class IdentityMapper
{
	[SuppressMessage("Mapper", "RMG020")]
	public partial UserLoginInfoDto UserToUserLoginInfoDto(User user);

	public partial AuthenticateCommand AuthenticateRequestToAuthenticateCommand(AuthenticateRequest request);
}
