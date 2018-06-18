using System;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CHC.Consent.DependencyInjection
{
    public static class PersonIdentifierRegistryServiceExtensions
    {
        public static IServiceCollection AddPersonIdentifiers(
            this IServiceCollection services,
            Action<PersonIdentifierRegistryOptions> setup
        )
        {
            var log = services.BuildServiceProvider().GetService<ILoggerProvider>()
                .CreateLogger("PersonIdentifierRegistry");
            
            var options = new PersonIdentifierRegistryOptions();
            
            setup(options);
            
            AddHandlers(services, options, log);

            AddTypeRegistry(services, options);

            AddListChecker(services, options);

            return services;
        }

        private static void AddListChecker(IServiceCollection services, PersonIdentifierRegistryOptions options)
        {
            services.AddSingleton<IPersonIdentifierListChecker>(options.CreateListChecker());
        }

        private static void AddTypeRegistry(IServiceCollection services, PersonIdentifierRegistryOptions options)
        {
            var registry = options.CreateTypeRegistry();

            services.AddSingleton(registry);
            services.AddSingleton<ITypeRegistry<IPersonIdentifier>>(registry);
        }

        private static void AddHandlers(IServiceCollection services, PersonIdentifierRegistryOptions options, ILogger log)
        {
            foreach (var description in options.IdentifierDescriptions)
            {
                var handlerType = typeof(IPersonIdentifierPersistanceHandler<>).MakeGenericType(description.IdentifierType);
                services.AddScoped(handlerType, description.PersistanceHandlerProvider);

                if (description.DisplayHandlerProvider == null)
                {
                    log.LogDebug("No display handler available for {0}", description.IdentifierType);
                    continue;
                }
                
                log.LogTrace("Adding display handler for {0}", description.IdentifierType);
                services.AddScoped(
                    typeof(IPersonIdentifierDisplayHandler<>).MakeGenericType(description.IdentifierType),
                    description.DisplayHandlerProvider
                );
            }
            
            services.AddScoped(typeof(PersonIdentifierPersistanceHandlerWrapper<>));
            services.AddScoped(typeof(PersonIdentifierDisplayHandlerWrapper<>));

            services.AddScoped<IIdentifierHandlerProvider, IdentifierHandlerProvider>();
        }
    }
}