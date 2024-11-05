using CommerceMono.Modules.Core.Configurations;
using CommerceMono.Modules.Core.Persistences;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceMono.Modules.Postgres;

public static class PostgresExtensions
{
	public static IServiceCollection AddNpgDbContext<TContext>(
		this IServiceCollection services)
		where TContext : DbContext, IDbContext
	{
		AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

		services.AddValidateOptions<PostgresOptions>();

		services.AddDbContext<TContext>((sp, options) =>
		{
			var postgresOptions = sp.GetRequiredService<PostgresOptions>();

			options.UseNpgsql(
				postgresOptions?.ConnectionString,
				dbOptions =>
				{
					dbOptions.MigrationsAssembly(typeof(TContext).Assembly.GetName().Name);
				}
			).UseSnakeCaseNamingConvention();
		});

		services.AddScoped<IDbContext>(provider => provider.GetRequiredService<TContext>());

		return services;
	}
}
