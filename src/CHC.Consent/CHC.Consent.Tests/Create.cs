using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
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


        public static IPersonIdentifier DateOfBirth(int year, int month, int day)
        {
            return DateOfBirth(new DateTime(year, month, day));
        }

        public static IPersonIdentifier DateOfBirth(DateTime value)
        {
            return new DateOfBirthIdentifier(value);
        }

        public static IPersonIdentifier NhsNumber(string nhsNumber) => new NhsNumberIdentifier(nhsNumber);

        public static IPersonIdentifier BradfordHospitalNumber(string hosptialNumber) => new BradfordHospitalNumberIdentifier(hosptialNumber);
    }
}