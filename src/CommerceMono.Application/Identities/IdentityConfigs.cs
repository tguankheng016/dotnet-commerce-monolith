using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using CommerceMono.Modules.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CommerceMono.Application.Identities;

public class IdentityConfigs
{
	public const string ApiVersionSet = "Identity";
}

public static class IdentityApiVersionSets
{
	public static ApiVersionSet GetApiVersionSet(this IEndpointRouteBuilder builder)
	{
		return builder.NewApiVersionSet(IdentityConfigs.ApiVersionSet)
			.ToLatestApiVersion()
			.Build();
	}
}