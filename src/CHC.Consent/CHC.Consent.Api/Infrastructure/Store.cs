using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.Api.Infrastructure
{
    /// <summary>
    /// Helper class to provide resolution of <see cref="IStore{T}"/> from <see cref="IStoreProvider"/> 
    /// </summary>
    /// <typeparam name="T">Type of the <see cref="IEntity"/> to be stored</typeparam>
    public class Store<T> : IStore<T> where T : class, IEntity
    {
        private IStore<T> inner;
        /// <inheritdoc />
        public Store(IStoreProvider storeProvider)
        {
            inner = storeProvider.Get<T>();
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() => inner.GetEnumerator();

        /// <inheritdoc />
        public Type ElementType => inner.ElementType;

        /// <inheritdoc />
        public Expression Expression => inner.Expression;

        /// <inheritdoc />
        public IQueryProvider Provider => inner.Provider;

        /// <inheritdoc />
        public T Add(T value) => inner.Add(value);

        /// <inheritdoc />
        public T Get(long id) => inner.Get(id);

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}