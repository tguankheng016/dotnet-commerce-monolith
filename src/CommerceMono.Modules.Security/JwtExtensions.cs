using CommerceMono.Modules.Core.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CommerceMono.Modules.Security;

public static class JwtExtensions
{
	public static IServiceCollection AddCustomJwtTokenHandler(this IServiceCollection services)
	{
		services.AddSingleton<TokenAuthConfiguration>();

		services.AddScoped<AccessSecurityTokenHandler>();
		services.AddScoped<ITokenKeyValidator, TokenKeyValidator>();
		services.AddScoped<ITokenSecurityStampValidator, TokenSecurityStampValidator>();
		services.AddScoped<IRefreshSecurityTokenHandler, RefreshSecurityTokenHandler>();

		return services;
	}

	public static IServiceCollection AddCustomJwtAuthentication(this IServiceCollection services)
	{
		var tokenAuthConfiguration = services.BuildServiceProvider().GetRequiredService<TokenAuthConfiguration>();

		services.AddAuthentication(
			options =>
			{
				options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
			.AddJwtBearer(options =>
			{
				options.Audience = tokenAuthConfiguration.Audience;

				options.TokenValidationParameters = new TokenValidationParameters
				{
					// The signing key must match!
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = tokenAuthConfiguration.SecurityKey,

					// Validate the JWT Issuer (iss) claim
					ValidateIssuer = true,
					ValidIssuer = tokenAuthConfiguration.Issuer,

					// Validate the JWT Audience (aud) claim
					ValidateAudience = true,
					ValidAudience = tokenAuthConfiguration.Audience,

					// Validate the token expiry
					ValidateLifetime = true,

					// If you want to allow a certain amount of clock drift, set that here
					ClockSkew = TimeSpan.Zero
				};

				options.Events = new JwtBearerEvents
				{
					OnTokenValidated = async context =>
					{
						var customTokenHandler = context.HttpContext.RequestServices.GetRequiredService<AccessSecurityTokenHandler>();
						var result = await customTokenHandler.ValidateTokenAsync(context.Principal!);

						if (!result)
						{
							context.Fail("invalid token");
						}

						return;
					},
					OnAuthenticationFailed = context =>
					{
						if (context.Exception is SecurityTokenExpiredException)
						{
							throw new UnAuthorizedException("The Token is expired.");
						}

						throw new UnAuthorizedException(context.Exception.Message);
					},
					OnForbidden = _ => throw new ForbiddenException("You are not authorized to access this resource.")
				};
			});

		return services;
	}

	public static IApplicationBuilder UseJwtTokenMiddleware(this WebApplication app)
	{
		return app.Use(async (ctx, next) =>
		{
			if (ctx.User.Identity?.IsAuthenticated != true)
			{
				var endpoint = ctx.GetEndpoint();
				if (endpoint is not null)
				{
					var metadata = endpoint.Metadata;

					var requiresAuthorization = metadata.OfType<AuthorizeAttribute>().Any();
					var allowAnonymous = metadata.OfType<AllowAnonymousAttribute>().Any();

					try
					{
						var result = await ctx.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
						if (result.Succeeded && result.Principal is not null)
						{
							ctx.User = result.Principal;
						}
					}
					catch (UnAuthorizedException)
					{
						if (requiresAuthorization && !allowAnonymous)
						{
							throw;
						}
					}
				}
			}

			await next();
		});
	}
}
