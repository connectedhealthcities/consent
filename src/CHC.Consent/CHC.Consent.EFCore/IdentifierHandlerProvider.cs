using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace CHC.Consent.EFCore
{
    public class IdentifierHandlerProvider : IIdentifierHandlerProvider
    {
        public IServiceProvider Services { get; }
        
        public IdentifierHandlerProvider(IServiceProvider services)
        {
            Services = services;
        }

        private class WrapperTypeCache
        {
            private readonly Type genericWrapperType;
            private Dictionary<Type, Type> wrapperTypeCache = new Dictionary<Type, Type>();

            public WrapperTypeCache(Type genericWrapperType)
            {
                if(!genericWrapperType.IsGenericTypeDefinition) throw new ArgumentException("Must be an open generic type", nameof(genericWrapperType));
                if(genericWrapperType.GetGenericArguments().Length != 1) throw new ArgumentException("Must have one type argument (Type`1[])", nameof(genericWrapperType));
                this.genericWrapperType = genericWrapperType;
            }

            public Type GetWrapperType(Type identifierType)
            {
                if(!wrapperTypeCache.ContainsKey(identifierType))
                    wrapperTypeCache.Add(identifierType, genericWrapperType.MakeGenericType(identifierType));
                return wrapperTypeCache[identifierType];
            }
        }

        private readonly WrapperTypeCache filterWrapperTypeCache = new WrapperTypeCache(typeof(IdentifierFilterWrapper<>)); 
        public IFilterWrapper GetFilter(IPersonIdentifier identifier)
        {
            var wrapperType = filterWrapperTypeCache.GetWrapperType(identifier.GetType());
            return (IFilterWrapper)Services.GetRequiredService(wrapperType);
        }

        private readonly WrapperTypeCache retrieverWrapperTypeCache = new WrapperTypeCache(typeof(IdentifierRetrieverWrapper<>));

        public IRetrieverWrapper GetRetriever(Type identifierType)
        {
            var wrapperType = retrieverWrapperTypeCache.GetWrapperType(identifierType);
            return (IRetrieverWrapper) Services.GetRequiredService(wrapperType);
        }
        
        private readonly WrapperTypeCache updaterWrapperTypeCache = new WrapperTypeCache(typeof(IdentifierUpdaterWrapper<>));
        public IUpdaterWrapper GetUpdater(Type identifierType)
        {
            var wrapperType = updaterWrapperTypeCache.GetWrapperType(identifierType);
            return (IUpdaterWrapper) Services.GetRequiredService(wrapperType);
        }
    }

    public class IdentifierUpdaterWrapper<T> : IUpdaterWrapper where T : IPersonIdentifier
    {
        private readonly IIdentifierUpdater<T> updater;

        public IdentifierUpdaterWrapper(IIdentifierUpdater<T> updater)
        {
            this.updater = updater;
        }

        public bool Update(PersonEntity person, IPersonIdentifier value, IStoreProvider stores)
        {
            return updater.Update(person, (T)value, stores);
        }
    }

    public class IdentifierRetrieverWrapper<T> : IRetrieverWrapper where T : IPersonIdentifier
    {
        private readonly IIdentifierRetriever<T> retriever;

        /// <inheritdoc />
        public IdentifierRetrieverWrapper(IIdentifierRetriever<T> retriever)
        {
            this.retriever = retriever;
        }


        /// <param name="person"></param>
        /// <param name="stores"></param>
        /// <inheritdoc />
        public IEnumerable<IPersonIdentifier> Get(PersonEntity person, IStoreProvider stores)
        {
            return retriever.Get(person, stores).Cast<IPersonIdentifier>();
        }
    }

    public class IdentifierFilterWrapper<T> : IFilterWrapper where T : IPersonIdentifier
    {
        private readonly IIdentifierFilter<T> filter;

        /// <inheritdoc />
        public IdentifierFilterWrapper(IIdentifierFilter<T> filter)
        {
            this.filter = filter;
        }


        /// <inheritdoc />
        public IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people, IPersonIdentifier identifier, IStoreProvider storeProvider)
        {
            return filter.Filter(people, (T) identifier, storeProvider);
        }
    }
}