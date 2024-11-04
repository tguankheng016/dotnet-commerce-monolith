using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace CommerceMono.Modules.Web;

public static class SwaggerExtensions
{
	public static IServiceCollection AddCustomSwagger(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddSwaggerGen(options =>
		{
			var bearerScheme = new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.Http,
				Name = JwtBearerDefaults.AuthenticationScheme,
				Scheme = JwtBearerDefaults.AuthenticationScheme,
				Reference = new() { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme }
			};

			options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, bearerScheme);

			options.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type=ReferenceType.SecurityScheme,
								Id="Bearer"
							}
						},
						new string[]{}
					}
				});
		});

		services.ConfigureOptions<ConfigureSwaggerOptions>();

		return services;
	}

	public static IApplicationBuilder UseCustomSwagger(this WebApplication app)
	{
		app.UseSwagger();
		app.UseSwaggerUI(
			options =>
			{
				var descriptions = app.DescribeApiVersions();

				// build a swagger endpoint for each discovered API version
				foreach (var description in descriptions)
				{
					var url = $"/swagger/{description.GroupName}/swagger.json";
					var name = description.GroupName.ToUpperInvariant();
					options.SwaggerEndpoint(url, name);
				}
			});

		return app;
	}
}
