using CommerceMono.Modules.Core.Configurations;
using EasyCaching.Redis;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceMono.Modules.Caching;

public static class CachingExtensions
{
	public static IServiceCollection AddCustomEasyCaching(this IServiceCollection services)
	{
		services.AddValidateOptions<RedisOptions>();
		var redisOptions = services.BuildServiceProvider().GetRequiredService<RedisOptions>();

		services.AddEasyCaching(option =>
		{
			if (redisOptions.Enabled)
			{
				option.UseRedis(
					config =>
					{
						config.DBConfig = new RedisDBOptions
						{
							Configuration = $"{redisOptions?.Host}:{redisOptions?.Port}"
						};
						config.SerializerName = nameof(Newtonsoft);
					},
					nameof(CacheProviderType.Redis)
				);
			}
			else
			{
				option.UseInMemory(
					config =>
					{
						config.SerializerName = nameof(Newtonsoft);
					},
					nameof(CacheProviderType.InMemory)
				);
			}

			option.WithJson(
				jsonSerializerSettingsConfigure: x =>
				{
					x.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None;
				},
				nameof(Newtonsoft)
			);
		});

		services.AddSingleton<ICacheManager, CacheManager>();

		return services;
	}
}

