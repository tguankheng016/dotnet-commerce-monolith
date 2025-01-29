using CommerceMono.Api;
using CommerceMono.Modules.Postgres;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Testcontainers.PostgreSql;
using Xunit.Abstractions;

namespace CommerceMono.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<IApplicationRoot>, IAsyncLifetime
{
	public ITestOutputHelper? Output { get; set; }

	public readonly PostgreSqlContainer DatabaseContainer = new PostgreSqlBuilder()
		.WithUsername("workshop")
		.WithPassword("password")
		.WithDatabase("mydb")
		.Build();

	public async Task InitializeAsync()
	{
		await DatabaseContainer.StartAsync();
	}

	public async new Task DisposeAsync()
	{
		await DatabaseContainer.StopAsync();
	}

	protected override IHost CreateHost(IHostBuilder builder)
	{
		builder.UseSerilog(
			(ctx, loggerConfiguration) =>
			{
				if (Output is not null)
				{
					loggerConfiguration.WriteTo.TestOutput(
						Output,
						LogEventLevel.Error,
						"{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}"
					);
				}
			}
		);

		return base.CreateHost(builder);
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
					ConnectionString = DatabaseContainer.GetConnectionString()
				};

				services.AddSingleton(newPostgresOptions);
			}
		});
	}
}