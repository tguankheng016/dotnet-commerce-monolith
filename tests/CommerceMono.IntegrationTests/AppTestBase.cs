using CommerceMono.Application.Data;
using CommerceMono.Modules.Caching;
using EasyCaching.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace CommerceMono.IntegrationTests;

public abstract class AppTestBase
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
		TestWebApplicationFactory webAppFactory
	)
	{
		ApiFactory = webAppFactory;
		webAppFactory.Output = testOutputHelper;
		Client = ApiFactory.CreateClient();
		var _scope = ApiFactory.Services.CreateScope();
		DbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
		CacheProvider = _scope.ServiceProvider.GetRequiredService<ICacheManager>().GetCachingProvider();
	}
}
