using CommerceMono.Application.Data;
using CommerceMono.Modules.Caching;
using EasyCaching.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace CommerceMono.IntegrationTests;

public class AppTestBase : IClassFixture<TestContainers>
{
	protected readonly TestWebApplicationFactory ApiFactory;
	protected readonly AppDbContext DbContext;
	protected readonly IEasyCachingProvider CacheProvider;
	protected readonly HttpClient Client;
	protected virtual string EndpointPrefix { get; } = "api";
	protected virtual string EndpointVersion { get; } = "v1";
	protected virtual string EndpointName { get; } = "";
	protected string Endpoint
	{
		get
		{
			return $"{EndpointPrefix}/{EndpointVersion}/{EndpointName}";
		}
	}

	public AppTestBase(
		ITestOutputHelper testOutputHelper,
		TestContainers testContainers
	)
	{
		ApiFactory = new TestWebApplicationFactory(testOutputHelper, testContainers);
		Client = ApiFactory.CreateClient();
		var _scope = ApiFactory.Services.CreateScope();
		DbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
		CacheProvider = _scope.ServiceProvider.GetRequiredService<ICacheManager>().GetCachingProvider();
	}
}
