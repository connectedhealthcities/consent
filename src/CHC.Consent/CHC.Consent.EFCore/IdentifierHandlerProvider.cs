using System;
using System.Collections.Generic;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore.Identity;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace CHC.Consent.EFCore
{
    /// <summary>
    /// <para>
    /// Retrieves <see cref="PersonIdentifierPersistanceHandlerWrapper{TIdentifier}"/> from a <see cref="IServiceProvider"/>
    /// by <see cref="Type"/> of Identifier
    /// </para> 
    /// </summary>
    public class IdentifierHandlerProvider : IIdentifierHandlerProvider
    {
        private IServiceProvider Services { get; }

        public IdentifierHandlerProvider(IServiceProvider services)
        {
            Services = services;
        }

        private class WrapperTypeCache
        {
            private readonly Type genericWrapperType;
            private readonly Dictionary<Type, Type> wrapperTypeCache = new Dictionary<Type, Type>();

            public WrapperTypeCache(Type genericWrapperType)
            {
                if (!genericWrapperType.IsGenericTypeDefinition)
                    throw new ArgumentException("Must be an open generic type", nameof(genericWrapperType));
                if (genericWrapperType.GetGenericArguments().Length != 1)
                    throw new ArgumentException("Must have one type argument (Type`1[])", nameof(genericWrapperType));
                this.genericWrapperType = genericWrapperType;
            }

            public Type GetWrapperType(Type identifierType)
            {
                if (!wrapperTypeCache.ContainsKey(identifierType))
                    wrapperTypeCache.Add(identifierType, genericWrapperType.MakeGenericType(identifierType));
                return wrapperTypeCache[identifierType];
            }
        }

        private readonly WrapperTypeCache persistanceHandlerWrapperTypeCache =
            new WrapperTypeCache(typeof(PersonIdentifierPersistanceHandlerWrapper<>));

        private readonly WrapperTypeCache displayHandlerWrapperTypeCache =
            new WrapperTypeCache(typeof(PersonIdentifierDisplayHandlerWrapper<>));


        private static void EnsureIsPersonIdentifierType(Type identifierType)
        {
            if (!identifierType.IsSubtypeOf<IPersonIdentifier>())
                throw new ArgumentException(
                    $"{identifierType} is not a {nameof(IPersonIdentifier)}",
                    nameof(identifierType));
        }

        public IPersonIdentifierPersistanceHandler GetPersistanceHandler(Type identifierType)
        {
            EnsureIsPersonIdentifierType(identifierType);

            var wrapperType = persistanceHandlerWrapperTypeCache.GetWrapperType(identifierType);
            return (IPersonIdentifierPersistanceHandler) Services.GetRequiredService(wrapperType);
        }

        public IPersonIdentifierDisplayHandler GetDisplayHandler(Type identifierType)
        {
            EnsureIsPersonIdentifierType(identifierType);

            var wrapperType = displayHandlerWrapperTypeCache.GetWrapperType(identifierType);
            return (IPersonIdentifierDisplayHandler) Services.GetService(wrapperType) ??
                   throw new InvalidOperationException($"No display handler registered for {identifierType}");
        }
    }
}