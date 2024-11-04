using System.Reflection;
using System.Threading.RateLimiting;
using CommerceMono.Application.Data;
using CommerceMono.Application.Data.Seed;
using CommerceMono.Application.Identities.Services;
using CommerceMono.Application.Roles.Models;
using CommerceMono.Application.Users.Models;
using CommerceMono.Logging;
using CommerceMono.Modules.Caching;
using CommerceMono.Modules.Core.Dependencies;
using CommerceMono.Modules.Core.EfCore;
using CommerceMono.Modules.Core.Exceptions;
using CommerceMono.Modules.Core.Persistences;
using CommerceMono.Modules.Dapper;
using CommerceMono.Modules.Postgres;
using CommerceMono.Modules.Security;
using CommerceMono.Modules.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace CommerceMono.Application.Startup;

public static class InfrastructureExtensions
{
	public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder, Assembly assembly)
	{
		var configuration = builder.Configuration;
		var env = builder.Environment;
		assembly = typeof(InfrastructureExtensions).Assembly;

		builder.Services.AddDefaultDependencyInjection(assembly);

		builder.Services.AddScoped<IDataSeeder, DataSeeder>();
		builder.Services.AddScoped<ITokenKeyDbValidator, TokenKeyDbValidator>();
		builder.Services.AddScoped<ITokenSecurityStampDbValidator, TokenSecurityStampDbValidator>();

		builder.Services.AddRateLimiter(options =>
		{
			options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
				RateLimitPartition.GetFixedWindowLimiter(
					partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
					factory: partition => new FixedWindowRateLimiterOptions
					{
						AutoReplenishment = true,
						PermitLimit = 100,
						QueueLimit = 0,
						Window = TimeSpan.FromMinutes(1)
					}));
		});

		// Setup Minimal API
		builder.Services.AddMinimalEndpoints(assembly);
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddControllers();

		builder.Services.AddSwaggerGen();

		builder.Services.AddNpgDbContext<AppDbContext>();
		builder.Services.AddCustomDapper();

		builder.AddCustomSerilog(env);

		builder.Services.AddCustomEasyCaching();

		builder.Services.AddProblemDetails();

		builder.Services.AddIdentity<User, Role>(config =>
			{
				config.Password.RequiredLength = 6;
				config.Password.RequireDigit = false;
				config.Password.RequireNonAlphanumeric = false;
				config.Password.RequireUppercase = false;
			}
		).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

		builder.Services.AddCustomJwtTokenHandler();
		builder.Services.AddCustomJwtAuthentication();

		builder.Services.Configure<ForwardedHeadersOptions>(options =>
		{
			options.ForwardedHeaders =
				ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
		});

		return builder;
	}

	public static WebApplication UseInfrastructure(this WebApplication app)
	{
		app.UseForwardedHeaders();

		app.UseCustomProblemDetails();

		app.UseMigration<AppDbContext>();

		app.UseRateLimiter();

		app.UseAuthentication();

		app.UseJwtTokenMiddleware();

		app.UseAuthorization();

		// TODO: Wait Scalar To Fix Empty Body Issue
		app.UseSwagger(options =>
		{
			options.RouteTemplate = "/openapi/{documentName}.json";
		});

		app.MapScalarApiReference();

		app.MapMinimalEndpoints();

		return app;
	}
}
