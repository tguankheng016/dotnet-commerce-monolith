
using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CommerceMono.Application.Identities.Features.Authenticating;

public static class AuthenticateVersionSets
{
	public static ApiVersionSet GetApiVersionSet(this IEndpointRouteBuilder builder)
	{
		return builder.NewApiVersionSet("Authenticate")
			.HasDeprecatedApiVersion(1.0)
			.HasApiVersion(2.0)
			.Build();
	}
}
