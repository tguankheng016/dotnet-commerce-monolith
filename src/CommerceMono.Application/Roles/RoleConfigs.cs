using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CommerceMono.Application.Roles;

public class RoleConfigs
{
	public const string ApiVersionSet = "Role";
}

public static class RoleApiVersionSets
{
	public static ApiVersionSet GetApiVersionSet(this IEndpointRouteBuilder builder)
	{
		return builder.NewApiVersionSet(RoleConfigs.ApiVersionSet)
			.HasApiVersion(1.0)
			.Build();
	}
}
