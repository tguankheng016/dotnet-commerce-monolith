
using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CommerceMono.Application.Identities.Features.GettingCurrentSession;

public static class GetCurrentSessionVersionSets
{
    public static ApiVersionSet GetApiVersionSet(this IEndpointRouteBuilder builder)
    {
        return builder.NewApiVersionSet("GetCurrentSession")
            .HasApiVersion(1.0)
            .Build();
    }
}
