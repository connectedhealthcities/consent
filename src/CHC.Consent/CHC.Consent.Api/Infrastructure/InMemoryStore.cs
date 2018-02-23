using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.Api.Infrastructure
{
    public class InMemoryStore<T> : IStore<T> where T : IEntity
    {
        public List<T> Contents { get; }
        private readonly IQueryable<T> contentsQueryable;

        public delegate void ItemAdded(InMemoryStore<T> store, T item);

        public event ItemAdded OnItemAdded;

        public InMemoryStore()
        {
            Contents = new List<T>();
            contentsQueryable = Contents.AsQueryable();
        }

        public T Add(T item)
        {
            Contents.Add(item);
            OnItemAdded?.Invoke(this, item);
            return item;
        }

        /// <inheritdoc />
        public T Get(long id)
        {
            return Contents.SingleOrDefault(_ => _.Id == id);
        }

        public IEnumerator<T> GetEnumerator() => Contents.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public Type ElementType => contentsQueryable.ElementType;

        /// <inheritdoc />
        public Expression Expression => contentsQueryable.Expression;

        /// <inheritdoc />
        public IQueryProvider Provider => contentsQueryable.Provider;
    }
}