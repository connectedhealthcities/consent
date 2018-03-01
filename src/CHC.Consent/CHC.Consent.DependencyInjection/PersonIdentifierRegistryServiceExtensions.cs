using System;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using Microsoft.Extensions.DependencyInjection;

namespace CHC.Consent.DependencyInjection
{
    public static class PersonIdentifierRegistryServiceExtensions
    {
        public static IServiceCollection AddPersonIdentifiers(
            this IServiceCollection services,
            Action<PersonIdentifierRegistryOptions> setup
        )
        {
            var options = new PersonIdentifierRegistryOptions();
            
            setup(options);
            
            AddHandlers(services, options);

            AddTypeRegistry(services, options);

            AddListChecker(services, options);

            return services;
        }

        private static void AddListChecker(IServiceCollection services, PersonIdentifierRegistryOptions options)
        {
            var checker = new PersonIdentifierListChecker();
            foreach (var description in options.IdentifierDescriptions)
            {
                checker.Add(description.IdentifierType, description.CanHaveDuplicates);
            }

            services.AddSingleton<IPersonIdentifierListChecker>(checker);
        }

        private static void AddTypeRegistry(IServiceCollection services, PersonIdentifierRegistryOptions options)
        {
            var registry = new TypeRegistry<IIdentifier, IdentifierAttribute>();
            foreach (var description in options.IdentifierDescriptions)
            {
                registry.Add(description.IdentifierType, description.TypeName);
            }

            services.AddSingleton(registry);
            services.AddSingleton<ITypeRegistry<IIdentifier>>(registry);
        }

        private static void AddHandlers(IServiceCollection services, PersonIdentifierRegistryOptions options)
        {
            foreach (var description in options.IdentifierDescriptions)
            {
                var filterType = typeof(IIdentifierFilter<>).MakeGenericType(description.IdentifierType);
                services.AddScoped(filterType, description.FilterProvider);

                var updaterType = typeof(IIdentifierUpdater<>).MakeGenericType(description.IdentifierType);
                services.AddScoped(updaterType, description.UpdaterProvider);

                var retrieverType = typeof(IIdentifierRetriever<>).MakeGenericType(description.IdentifierType);
                services.AddScoped(retrieverType, description.RetrieverProvider);
            }

            services.AddScoped(typeof(IdentifierFilterWrapper<>));
            services.AddScoped(typeof(IdentifierRetrieverWrapper<>));
            services.AddScoped(typeof(IdentifierUpdaterWrapper<>));

            services.AddScoped<IIdentifierHandlerProvider, IdentifierHandlerProvider>();
        }
    }
}