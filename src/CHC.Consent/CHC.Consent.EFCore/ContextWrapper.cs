using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CHC.Consent.EFCore
{
    public class ContextWrapper : IStoreProvider, IQueryableProvider
    {
        private readonly ConsentContext context;

        public ContextWrapper(ConsentContext context)
        {
            this.context = context;
        }

        class ContextStore<T> : IStore<T> where T : class, IEntity
        {
            private readonly DbSet<T> set;

            /// <inheritdoc />
            public ContextStore(DbSet<T> set)
            {
                this.set = set;
            }

            public IEnumerator<T> GetEnumerator() => ((IQueryable<T>) set).GetEnumerator();

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <inheritdoc />
            public Type ElementType => ((IQueryable) set).ElementType;

            /// <inheritdoc />
            public Expression Expression => ((IQueryable) set).Expression;

            /// <inheritdoc />
            public IQueryProvider Provider => ((IQueryable) set).Provider;

            /// <inheritdoc />
            public T Add(T value)
            {
                return set.Add(value).Entity;
            }

            /// <inheritdoc />
            public T Get(long id)
            {
                return set.Find(id);
            }
        }

        IQueryable<T> IQueryableProvider.Get<T>() => context.Set<T>();
        /// <inheritdoc />
        IStore<T> IStoreProvider.Get<T>() => new ContextStore<T>(context.Set<T>());
    }
}