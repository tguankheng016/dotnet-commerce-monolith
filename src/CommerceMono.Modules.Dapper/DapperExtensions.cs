using System;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceMono.Modules.Dapper;

public static class DapperExtensions
{
	public static IServiceCollection AddCustomDapper(this IServiceCollection services, string connectionString)
	{
		services.AddSingleton<IDbConnectionFactory>(_ =>
		{
			return new NpgSqlDbConnectionFactory(connectionString);
		});

		return services;
	}
}
