using Microsoft.AspNetCore.Identity;

namespace CommerceMono.Application.Users.Models;

public class UserToken : IdentityUserToken<long>
{
    public virtual DateTimeOffset ExpireDate { get; set; }
}
