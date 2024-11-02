
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace CommerceMono.Modules.Web;

public static class MinimalApiExtensions
{
	public static IServiceCollection AddMinimalEndpoints(
		this IServiceCollection services,
		Assembly assemblies,
		ServiceLifetime lifetime = ServiceLifetime.Scoped)
	{
		services.Scan(scan => scan
			.FromAssemblies(assemblies)
			.AddClasses(classes => classes.AssignableTo(typeof(IMinimalEndpoint)))
			.UsingRegistrationStrategy(RegistrationStrategy.Append)
			.As<IMinimalEndpoint>()
			.WithLifetime(lifetime));

		return services;
	}

	/// <summary>
	/// Map Minimal Endpoints
	/// </summary>
	/// <name>builder.</name>
	/// <returns>IEndpointRouteBuilder.</returns>
	public static IEndpointRouteBuilder MapMinimalEndpoints(this IEndpointRouteBuilder builder)
	{
		var scope = builder.ServiceProvider.CreateScope();

		var endpoints = scope.ServiceProvider.GetServices<IMinimalEndpoint>();

		foreach (var endpoint in endpoints)
		{
			endpoint.MapEndpoint(builder);
		}

		return builder;
	}
}
