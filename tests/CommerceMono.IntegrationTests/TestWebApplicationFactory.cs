using CommerceMono.Api;
using CommerceMono.Application.Data;
using CommerceMono.Modules.Dapper;
using CommerceMono.Modules.Postgres;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;

namespace CommerceMono.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<IApplicationRoot>, IAsyncLifetime
{
	private readonly PostgreSqlContainer _databaseContainer = new PostgreSqlBuilder()
		.WithUsername("workshop")
		.WithPassword("password")
		.WithDatabase("mydb")
		.Build();

	public async Task InitializeAsync()
	{
		await _databaseContainer.StartAsync();
	}

	public new async Task DisposeAsync()
	{
		await _databaseContainer.StopAsync();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureTestServices(services =>
		{
			var descriptor = services
				.SingleOrDefault(s => s.ServiceType == typeof(PostgresOptions));

			if (descriptor is not null)
			{
				services.Remove(descriptor);

				var newPostgresOptions = new PostgresOptions()
				{
					ConnectionString = _databaseContainer.GetConnectionString()
				};

				services.AddSingleton(newPostgresOptions);
			}
		});
	}
}
