using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.Tests
{
    public class MockStore<T> : IStore<T> where T : IEntity
    {
        private readonly List<T> contents;
        private readonly List<T> additions = new List<T>();

        public IEnumerable<T> Contents => contents;
        public IEnumerable<T> Additions => additions;
        private IQueryable<T> ContentsAsQueryable => Contents.AsQueryable();

        /// <inheritdoc />
        public MockStore(params T[] contents) : this(contents.AsEnumerable())
        {
        }

        public MockStore(IEnumerable<T> contents)
        {
            this.contents = new List<T>(contents);
            
        }

        /// <inheritdoc />
        public T Add(T value)
        {
            contents.Add(value);
            additions.Add(value);
            return value;
        }

        /// <inheritdoc />
        public T Get(long id)
        {
            return contents.SingleOrDefault(_ => _.Id == id);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            return contents.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public Type ElementType => ContentsAsQueryable.ElementType;

        /// <inheritdoc />
        public Expression Expression => ContentsAsQueryable.Expression;

        /// <inheritdoc />
        public IQueryProvider Provider => ContentsAsQueryable.Provider;
    }
}