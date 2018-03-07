using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.Tests
{
    public static partial class Create
    {
        public static StoreBuilder<T> AMockStore<T>() where T : IEntity => new StoreBuilder<T>();

        public static IStore<T> AStore<T>(params T[] contents) where T : IEntity =>
            AMockStore<T>().WithContents(contents).Build();

        public class StoreBuilder<T> : Builder<MockStore<T>, StoreBuilder<T>> where T : IEntity
        {
            private T[] contents = Array.Empty<T>();

            public StoreBuilder<T> WithContents(params T[] newContents) => WithContents(newContents.AsEnumerable());

            private StoreBuilder<T> WithContents(IEnumerable<T> newContents)
            {
                return Copy(@new => @new.contents = Clone(newContents.ToArray()));
            }

            /// <inheritdoc />
            public override MockStore<T> Build()
            {
                return new MockStore<T>(contents);
            }
        }
    }
}