using System;
using EasyCaching.Core;

namespace CommerceMono.Modules.Caching;

public class CacheManager : ICacheManager
{
	private readonly IEasyCachingProviderFactory _factory;
	private readonly RedisOptions _redisOptions;

	public CacheManager(
		IEasyCachingProviderFactory factory,
		RedisOptions redisOptions
	)
	{
		_factory = factory;
		_redisOptions = redisOptions;
	}

	public IEasyCachingProvider GetCachingProvider()
	{
		if (!_redisOptions.Enabled)
		{
			return _factory.GetCachingProvider(nameof(CacheProviderType.InMemory));
		}

		return _factory.GetCachingProvider(nameof(CacheProviderType.Redis));
	}
}

public interface ICacheManager
{
	IEasyCachingProvider GetCachingProvider();
}