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
            var registry = new TypeRegistry<IPersonIdentifier, IdentifierAttribute>();
            foreach (var description in options.IdentifierDescriptions)
            {
                registry.Add(description.IdentifierType, description.TypeName);
            }

            services.AddSingleton(registry);
            services.AddSingleton<ITypeRegistry<IPersonIdentifier>>(registry);
        }

        private static void AddHandlers(IServiceCollection services, PersonIdentifierRegistryOptions options)
        {
            foreach (var description in options.IdentifierDescriptions)
            {
                var handlerType = typeof(IPersonIdentifierHandler<>).MakeGenericType(description.IdentifierType);
                services.AddScoped(handlerType, description.HandlerProvider);
            }

            services.AddScoped(typeof(PersonIdentifierHandlerWrapper<>));

            services.AddScoped<IIdentifierHandlerProvider, IdentifierHandlerProvider>();
        }
    }
}