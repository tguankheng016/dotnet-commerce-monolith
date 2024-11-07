using Asp.Versioning;
using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceMono.Modules.Web;

public static class ApiVersioningExtensions
{
	public static void AddCustomVersioning(this IServiceCollection services)
	{
		services.AddApiVersioning(options =>
		{
			options.DefaultApiVersion = new ApiVersion(1, 0);
			options.AssumeDefaultVersionWhenUnspecified = true;

			// Add the headers "api-supported-versions" and "api-deprecated-versions"
			options.ReportApiVersions = true;

			// Defines how an API version is read from the current HTTP request
			options.ApiVersionReader = ApiVersionReader.Combine(
				new HeaderApiVersionReader("api-version"),
				new UrlSegmentApiVersionReader());
		})
		.AddApiExplorer(options =>
		{
			// add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
			// note: the specified format code will format the version as "'v'major[.minor][-status]"
			options.GroupNameFormat = "'v'VVV";

			// note: this option is only necessary when versioning by url segment. the SubstitutionFormat
			// can also be used to control the format of the API version in route templates
			options.SubstituteApiVersionInUrl = true;
		});
	}

	public static RouteHandlerBuilder HasLatestApiVersion(this RouteHandlerBuilder builder, SupportedApiVersions fromApiVersion = SupportedApiVersions.V1, SupportedApiVersions? toApiVersion = null)
	{
		var supportedApiVersions = Enum.GetValues(typeof(SupportedApiVersions)).Cast<int>().ToList();

		supportedApiVersions = supportedApiVersions.Where(x => x >= (int)fromApiVersion).ToList();

		if (toApiVersion is not null)
		{
			supportedApiVersions = supportedApiVersions.Where(x => x <= (int)toApiVersion).ToList();
		}

		foreach (var apiVersion in supportedApiVersions)
		{
			builder.HasApiVersion(apiVersion);
		}

		return builder;
	}

	public static ApiVersionSetBuilder ToLatestApiVersion(this ApiVersionSetBuilder builder)
	{
		var supportedApiVersions = Enum.GetValues(typeof(SupportedApiVersions)).Cast<int>().ToList();
		var deprecatedApiVersions = new List<SupportedApiVersions>
		{
			SupportedApiVersions.V1
		};

		foreach (var apiVersion in supportedApiVersions)
		{
			if (!deprecatedApiVersions.Contains((SupportedApiVersions)apiVersion))
			{
				builder.HasApiVersion(apiVersion);
			}
			else
			{
				builder.HasDeprecatedApiVersion(apiVersion);
			}
		}

		return builder;
	}
}
