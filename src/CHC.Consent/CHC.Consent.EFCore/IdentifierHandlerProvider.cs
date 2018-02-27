using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    public class IdentifierHandlerProvider 
    {
        private readonly PersonIdentifierRegistry registry;

        public IdentifierHandlerProvider(PersonIdentifierRegistry registry)
        {
            this.registry = registry;
        }

        private static readonly Dictionary<Type, Type> FilterWrapperTypeCache = new Dictionary<Type, Type>();

        private static Type GetFilterWrapperType(Type identifierType)
        {
            if(!FilterWrapperTypeCache.ContainsKey(identifierType))
                FilterWrapperTypeCache.Add(identifierType, typeof(FilterWrapper<>).MakeGenericType(identifierType));
            return FilterWrapperTypeCache[identifierType];
        }

        public IFilterWrapper GetFilter(IIdentifier identifier)
        {
            var filterType = registry.GetFilterType(identifier.GetType());
            var filter = Activator.CreateInstance(filterType);
            var wrapper = (IFilterWrapper) Activator.CreateInstance(GetFilterWrapperType(identifier.GetType()), filter);
            return wrapper;
        }

        private class FilterWrapper<T> : IFilterWrapper where T : IIdentifier
        {
            private readonly IIdentifierFilter<T> filter;

            /// <inheritdoc />
            public FilterWrapper(IIdentifierFilter<T> filter)
            {
                this.filter = filter;
            }


            /// <inheritdoc />
            public IQueryable<PersonEntity> Filter(
                IQueryable<PersonEntity> people, IIdentifier identifier, IStoreProvider storeProvider)
            {
                return filter.Filter(people, (T) identifier, storeProvider);
            }
        }
        
        private class RetrieverWrapper<T> : IRetrieverWrapper where T : IIdentifier
        {
            private readonly IIdentifierRetriever<T> retriever;

            /// <inheritdoc />
            public RetrieverWrapper(IIdentifierRetriever<T> retriever)
            {
                this.retriever = retriever;
            }


            /// <param name="person"></param>
            /// <param name="stores"></param>
            /// <inheritdoc />
            public IEnumerable<IIdentifier> Get(PersonEntity person, IStoreProvider stores)
            {
                return retriever.Get(person, stores).Cast<IIdentifier>();
            }
        }

        private class UpdaterWrapper<T> : IUpdaterWrapper where T : IIdentifier
        {
            private readonly IIdentifierUpdater<T> updater;

            public UpdaterWrapper(IIdentifierUpdater<T> updater)
            {
                this.updater = updater;
            }

            public bool Update(PersonEntity person, IIdentifier value, IStoreProvider stores)
            {
                return updater.Update(person, (T)value, stores);
            }
        }
        
        private static readonly Dictionary<Type, Type> RetrieverWrapperTypeCache = new Dictionary<Type, Type>();

        private static Type GetRetrieverWapperType(Type identifierType)
        {
            if(!RetrieverWrapperTypeCache.ContainsKey(identifierType))
                RetrieverWrapperTypeCache.Add(identifierType, typeof(RetrieverWrapper<>).MakeGenericType(identifierType));
            return RetrieverWrapperTypeCache[identifierType];
        }

        public IRetrieverWrapper GetRetriever(Type identifierType)
        {
            var retrieverType = registry.GetRetrieverType(identifierType);
            var filter = Activator.CreateInstance(retrieverType);
            var wrapper = (IRetrieverWrapper) Activator.CreateInstance(GetRetrieverWapperType(identifierType), filter);
            return wrapper;
        }
        
        private static readonly Dictionary<Type, Type> UpdaterWrapperTypeCache = new Dictionary<Type, Type>();
        
        private static Type GetUpdaterWapperType(Type identifierType)
        {
            if(!UpdaterWrapperTypeCache.ContainsKey(identifierType))
                UpdaterWrapperTypeCache.Add(identifierType, typeof(UpdaterWrapper<>).MakeGenericType(identifierType));
            return UpdaterWrapperTypeCache[identifierType];
        }

        public IUpdaterWrapper GetUpdater(Type identifierType)
        {
            var updaterType = registry.GetUpdaterType(identifierType);
            var filter = Activator.CreateInstance(updaterType);
            var wrapper = (IUpdaterWrapper) Activator.CreateInstance(GetUpdaterWapperType(identifierType), filter);
            return wrapper;
        }
    }
}