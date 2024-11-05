using System.Text;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CommerceMono.Modules.Web;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
	private readonly IApiVersionDescriptionProvider _provider;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConfigureSwaggerOptions"/> class.
	/// </summary>
	/// <param name="provider">The <see cref="IApiVersionDescriptionProvider">provider</see> used to generate Swagger documents.</param>
	/// <param name="options"></param>
	public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
	{
		_provider = provider;
	}

	/// <inheritdoc />
	public void Configure(SwaggerGenOptions options)
	{
		// add a swagger document for each discovered API version
		// note: you might choose to skip or document deprecated API versions differently
		foreach (var description in _provider.ApiVersionDescriptions)
		{
			options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
		}
	}

	private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
	{
		var text = new StringBuilder("An example application with OpenAPI, Swashbuckle, and API versioning.");
		var info = new OpenApiInfo
		{
			Version = description.ApiVersion.ToString(),
			Title = "APIs",
			Description = "An application with Swagger, Swashbuckle, and API versioning.",
			Contact = new OpenApiContact { Name = "", Email = "" },
			License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
		};

		if (description.IsDeprecated)
		{
			text.Append("This API version has been deprecated.");
		}

		info.Description = text.ToString();

		return info;
	}
}
