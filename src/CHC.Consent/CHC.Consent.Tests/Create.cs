using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;
using FakeItEasy;

namespace CHC.Consent.Tests
{
    public static partial class Create
    {
        public abstract class Builder<T>
        {
            public abstract T Build();

            public static implicit operator Builder<T>(T reference)
            {
                return (ReferenceBuilder) reference;
            }

            public static implicit operator T(Builder<T> builder)
            {
                return builder.Build();
            }
            
            private class ReferenceBuilder : Builder<T>
            {
                private T reference;

                /// <inheritdoc />
                private ReferenceBuilder(T reference)
                {
                    this.reference = reference;
                }

                public static implicit operator ReferenceBuilder(T reference)
                {
                    return new ReferenceBuilder(reference);
                }

                /// <inheritdoc />
                public override T Build()
                {
                    return reference;
                }
            }
        }

        public abstract class Builder<TBuilt, TBuilder> : Builder<TBuilt>
        {
            protected TBuilder Copy(Action<TBuilder> change)
            {
                var copy = (TBuilder)MemberwiseClone();
                change(copy);
                return copy;
            }

            public static implicit operator TBuilt(Builder<TBuilt, TBuilder> builder)
            {
                if (builder == null)
                    throw new ArgumentNullException(nameof(builder), "Cannot build from a null builder");
                return builder.Build();
            }

            
            /// <summary>
            /// <para>Helper to clone arrays because cloning arrays is verbose</para>
            /// <para>Creates a shallow copy of <paramref name="source"/></para>
            /// </summary>
            protected static T[] Clone<T>(T[] source)
            {
                return (T[]) source.Clone();
            }
        }

        public static StoreBuilder<T> AMockStore<T>() where T : IEntity => new StoreBuilder<T>();

        public static IStore<T> AStore<T>(params T[] contents) where T : IEntity => AMockStore<T>().WithContents(contents).Build();
        
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
        
        

        public static IIdentifier DateOfBirth(int year, int month, int day)
        {
            return DateOfBirth(new DateTime(year, month, day));
        }

        public static IIdentifier DateOfBirth(DateTime value)
        {
            return new DateOfBirthIdentifier {DateOfBirth = value};
        }

        public static IIdentifier NhsNumber(string nhsNumber) => new NhsNumberIdentifier(nhsNumber);

        public static IIdentifier BradfordHospitalNumber(string hosptialNumber) => new BradfordHospitalNumberIdentifier(hosptialNumber);
    }
}