using Asp.Versioning;
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
}
