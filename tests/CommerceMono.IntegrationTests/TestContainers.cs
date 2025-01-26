using Testcontainers.PostgreSql;

namespace CommerceMono.IntegrationTests;

public class TestContainers : IAsyncLifetime
{
    public readonly PostgreSqlContainer DatabaseContainer = new PostgreSqlBuilder()
        .WithUsername("workshop")
        .WithPassword("password")
        .WithDatabase("mydb")
        .Build();

    public async Task InitializeAsync()
    {
        await DatabaseContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await DatabaseContainer.StopAsync();
    }
}