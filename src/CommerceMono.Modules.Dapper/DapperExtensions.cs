using System;
using CommerceMono.Modules.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceMono.Modules.Dapper;

public static class DapperExtensions
{
	public static IServiceCollection AddCustomDapper(this IServiceCollection services)
	{
		services.AddSingleton<IDbConnectionFactory>(sp =>
		{
			var postgresOptions = sp.GetRequiredService<PostgresOptions>();
			return new NpgSqlDbConnectionFactory(postgresOptions.ConnectionString);
		});

		return services;
	}
}
