using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CommerceMono.Modules.Core.Configurations;

public static class ConfigurationExtensions
{
    public static TModel GetOptions<TModel>(this IConfiguration configuration, string section) where TModel : new()
    {
        var model = new TModel();
        configuration.GetSection(section).Bind(model);
        return model;
    }

    public static TModel GetOptions<TModel>(this IServiceCollection service, string section) where TModel : new()
    {
        var model = new TModel();
        var configuration = service.BuildServiceProvider().GetService<IConfiguration>();
        configuration?.GetSection(section).Bind(model);
        return model;
    }

    public static TModel GetOptions<TModel>(this WebApplication app, string section) where TModel : new()
    {
        var model = new TModel();
        app.Configuration?.GetSection(section).Bind(model);
        return model;
    }

    public static void AddValidateOptions<TModel>(this IServiceCollection service) where TModel : class, new()
    {
        service.AddOptions<TModel>()
            .BindConfiguration(typeof(TModel).Name)
            .ValidateDataAnnotations();

        service.AddSingleton(x => x.GetRequiredService<IOptions<TModel>>().Value);
    }

    public static IServiceCollection ReplaceService<TSource, TImplementation>(this IServiceCollection services, ServiceLifetime lifeTime = ServiceLifetime.Scoped)
    {
        var descriptor = new ServiceDescriptor(typeof(TSource), typeof(TImplementation), lifeTime);
        services.Replace(descriptor);

        return services;
    }
}
