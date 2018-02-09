using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.IdentifierValues;
using CHC.Consent.Common.Infrastructure.Data;
using Xunit;

namespace CHC.Consent.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void NhsNumbersWithTheSameValueAreEqual()
        {
            const string nhsNumberValue = "44344323";
            Assert.StrictEqual(
                Create.NhsNumber(nhsNumberValue),
                Create.NhsNumber(nhsNumberValue));
        }
        
        [Fact]
        public void NhsNumbersWithTheDifferentValuesAreNotEqual()
        {
            Assert.NotStrictEqual(
                Create.NhsNumber("44344323"),
                Create.NhsNumber("87759567"));
        }

        [Fact]
        public void Test2()
        {
            var personOne = new Person {NhsNumber = "4333443"};
            var personTwo = new Person {NhsNumber = "6655666"};

            IdentityRepository repository = Create.AnIdentityRepository.WithPeople(personOne, personTwo);

            Assert.Equal(repository.FindPersonBy(Create.NhsNumber(personOne.NhsNumber)), personOne);
            Assert.Equal(repository.FindPersonBy(Create.NhsNumber(personTwo.NhsNumber)), personTwo);
            Assert.Equal(repository.FindPersonBy(Create.NhsNumber("7787773")), null);
        }
        
        [Fact]
        public void Test3()
        {
            var personOne = new Person().WithBradfordHosptialNumbers("212925", "990099");
            var personTwo = new Person().WithBradfordHosptialNumbers("6655666");

            IdentityRepository repository = Create.AnIdentityRepository.WithPeople(personOne, personTwo);

            Assert.Equal(repository.FindPersonBy(Create.BradfordHospitalNumber("212925")), personOne);
            Assert.Equal(repository.FindPersonBy(Create.BradfordHospitalNumber("990099")), personOne);
            Assert.Equal(repository.FindPersonBy(Create.BradfordHospitalNumber("6655666")), personTwo);
            Assert.Equal(repository.FindPersonBy(Create.BradfordHospitalNumber("7787773")), null);
            
        }

        [Fact]
        public void Test4()
        {
            var personOne = new Person{NhsNumber = "45"}.WithBradfordHosptialNumbers("7");
            
            IdentityRepository repository = Create.AnIdentityRepository.WithPeople(personOne);

            var found = repository.FindPersonBy(
                Create.NhsNumber(personOne.NhsNumber),
                Create.BradfordHospitalNumber("7")
            );
            
            Assert.Equal(personOne, found);
        }
    }

    public static class PersonTestHelpers
    {
        public static Person WithBradfordHosptialNumbers(this Person person, params string[] hospitalNumbers)
        {
            foreach (var hospitalNumber in hospitalNumbers)
            {
                person.AddHospitalNumber(hospitalNumber);
            }

            return person;
        }
    }

    public static partial class Create
    {
        public static Identifier NhsNumber(string value) => IdentifierType.NhsNumber.Parse(value);

        public static Identifier BradfordHospitalNumber(string hosptialNumber) =>
            IdentifierType.BradfordHospitalNumber.Parse(hosptialNumber);

        public static IndentityRepositoryBuilder AnIdentityRepository => new IndentityRepositoryBuilder();

        public abstract class Builder<TBuilt, TBuilder>
        {
            protected TBuilder Copy(Action<TBuilder> change)
            {
                var copy = (TBuilder)MemberwiseClone();
                change(copy);
                return copy;
            }

            public static implicit operator TBuilt(Builder<TBuilt, TBuilder> builder)
            {
                return builder.Build();
            }

            protected abstract TBuilt Build();

            /// <summary>
            /// <para>Helper to clone arrays because cloning arrays is verbose</para>
            /// <para>Creates a shallow copy of <paramref name="source"/></para>
            /// </summary>
            protected static T[] Clone<T>(T[] source)
            {
                return (T[]) source.Clone();
            }
        }

        public class MockStore<T> : IStore<T>
        {
            private readonly List<T> contents;
            private readonly IQueryable<T> contentsAsQueryable;
            private readonly List<T> additions = new List<T>();

            public IEnumerable<T> Contents => contents;
            public IEnumerable<T> Additions => additions;

            /// <inheritdoc />
            public MockStore(params T[] contents) : this(contents.AsEnumerable())
            {
            }

            public MockStore(IEnumerable<T> contents)
            {
                this.contents = new List<T>(contents);
                contentsAsQueryable = this.contents.AsQueryable();
            }

            /// <inheritdoc />
            public T Add(T value)
            {
                contents.Add(value);
                additions.Add(value);
                return value;
            }

            /// <inheritdoc />
            public IEnumerator<T> GetEnumerator()
            {
                return contents.GetEnumerator();
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <inheritdoc />
            public Type ElementType => contentsAsQueryable.ElementType;

            /// <inheritdoc />
            public Expression Expression => contentsAsQueryable.Expression;

            /// <inheritdoc />
            public IQueryProvider Provider => contentsAsQueryable.Provider;
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
            protected override MockStore<T> Build()
            {
                return new MockStore<T>(contents);
            }
        }
        
        public class IndentityRepositoryBuilder : Builder<IdentityRepository, IndentityRepositoryBuilder>
        {
            private Person[] people = Array.Empty<Person>();
            private IStore<Person> peopleStore = null;
            private IdentifierType[] identifierTypes = Array.Empty<IdentifierType>();

            public IndentityRepositoryBuilder WithPeople(params Person[] newPeople)
            {
                return Copy(change: @new => @new.people = Clone(newPeople));
            }

            protected override IdentityRepository Build()
            {
                return new IdentityRepository(
                    peopleStore ?? new MockStore<Person>(people),
                    identifierTypes.AsQueryable()
                );
            }

            public IndentityRepositoryBuilder WithIdentifierTypes(params IdentifierType[] newIdentifierTypes)
            {
                return Copy(change: @new => @new.identifierTypes = Clone(newIdentifierTypes));
            }

            public IndentityRepositoryBuilder WithPeopleStore(IStore<Person> newPeopleStore)
            {
                return Copy(change: @new => @new.peopleStore = newPeopleStore);
            }
        }

        public static Identifier DateOfBirth(int year, int month, int day)
        {
            return DateOfBirth(new DateTime(year, month, day));
        }

        public static Identifier DateOfBirth(DateTime value)
        {
            return new Identifier(
                IdentifierType.DateOfBirth,
                IdentifierType.DateOfBirth.ValueType,
                new DateIdentifierValue(value));
        }
    }
}