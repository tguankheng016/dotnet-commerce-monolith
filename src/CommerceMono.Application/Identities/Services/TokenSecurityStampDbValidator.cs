using CommerceMono.Application.Users.Models;
using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Security;
using EasyCaching.Core;
using Microsoft.AspNetCore.Identity;

namespace CommerceMono.Application.Identities.Services;

public class TokenSecurityStampDbValidator : ITokenSecurityStampDbValidator
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IEasyCachingProvider _cacheProvider;

    public TokenSecurityStampDbValidator(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ICacheManager cacheManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _cacheProvider = cacheManager.GetCachingProvider();
    }

    public async Task<bool> ValidateSecurityStampFromDbAsync(string cacheKey, string userId, string securityStamp)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        //cache last requested value
        await SetSecurityStampCacheAsync(cacheKey, user.SecurityStamp!);

        return await _signInManager.ValidateSecurityStampAsync(user, securityStamp);
    }

    private async Task SetSecurityStampCacheAsync(string cacheKey, string securityStamp)
    {
        await _cacheProvider.SetAsync(cacheKey, securityStamp, TimeSpan.FromHours(1));
    }
}
