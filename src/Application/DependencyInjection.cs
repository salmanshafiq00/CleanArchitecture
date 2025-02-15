using System.Reflection;
using Application.Common.Behaviours;
using Application.Common.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        //services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(RequestLoggingBehaviour<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            //cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));

            // Distributed caching
            //cfg.AddOpenBehavior(typeof(QueryCachingBehaviour<,>));
            //cfg.AddOpenBehavior(typeof(CacheInvalidationBehaviour<,>));

            // In-memory caching
            //cfg.AddOpenBehavior(typeof(InMemoryCachingBehaviour<,>));
            //cfg.AddOpenBehavior(typeof(InMemoryCacheInvalidationBehaviour<,>));

            // Lazy In-memory caching
            cfg.AddOpenBehavior(typeof(LazyCachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(LazyCacheInvalidationBehavior<,>));


            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));
        });

        // Register mapping configurations
        MappingConfigurations.ConfigureMappings();

        return services;
    }
}
