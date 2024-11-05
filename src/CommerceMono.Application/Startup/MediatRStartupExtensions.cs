using CommerceMono.Logging;
using CommerceMono.Modules.Core.EFCore;
using CommerceMono.Modules.Core.Validations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceMono.Application.Startup;

public static class MediatRStartupExtensions
{
    public static IServiceCollection AddCustomMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(MediatRStartupExtensions).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(EFTransactionBehavior<,>));

        return services;
    }
}
