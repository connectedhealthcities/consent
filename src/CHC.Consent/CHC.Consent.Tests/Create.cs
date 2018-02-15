using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.Tests
{
    public static partial class Create
    {
        public static IndentityRepositoryBuilder AnIdentityRepository => new IndentityRepositoryBuilder();
        public static PersonBuilder Person => new PersonBuilder();

        public class PersonBuilder : Builder<Person, PersonBuilder>
        {
            private string[] hospitalNumbers = Array.Empty<string>();

            /// <inheritdoc />
            public override Person Build()
            {
                var person = new Person();
                foreach (var hospitalNumber in hospitalNumbers)
                {
                    person.AddHospitalNumber(hospitalNumber);
                }

                return person;
            }

            public PersonBuilder WithHospitalNumbers(params string[] newHospitalNumbers)
            {
                return Copy(change: @new => @new.hospitalNumbers = Clone(newHospitalNumbers));
            }
        }

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

        public static StoreBuilder<T> AMockStore<T>() => new StoreBuilder<T>();
        
        public class StoreBuilder<T> : Builder<MockStore<T>, StoreBuilder<T>>
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
        
        public class IndentityRepositoryBuilder : Builder<IdentityRepository, IndentityRepositoryBuilder>
        {
            private Person[] people = Array.Empty<Person>();
            private IStore<Person> peopleStore = null;

            public IndentityRepositoryBuilder WithPeople(params Person[] newPeople)
            {
                return Copy(change: @new => @new.people = Clone(newPeople));
            }

            public override IdentityRepository Build()
            {
                return new IdentityRepository(
                    peopleStore ?? new MockStore<Person>(people)
                );
            }

            public IndentityRepositoryBuilder WithPeopleStore(IStore<Person> newPeopleStore)
            {
                return Copy(change: @new => @new.peopleStore = newPeopleStore);
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