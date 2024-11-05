using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CommerceMono.Application.Identities.Features.SigningOut;

public static class SignOutVersionSets
{
    public static ApiVersionSet GetApiVersionSet(this IEndpointRouteBuilder builder)
    {
        return builder.NewApiVersionSet("SignOut")
            .HasApiVersion(1.0)
            .Build();
    }
}