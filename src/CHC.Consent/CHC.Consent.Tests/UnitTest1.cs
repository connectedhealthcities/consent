using System;
using System.Linq;
using CHC.Consent.Common;
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
        public void Test1()
        {
            var nhsNumber = "123332332";
            var aPerson = new Person { NhsNumber = nhsNumber, };

            var nhsNumberIdentifier = Create.NhsNumber(nhsNumber);

            var identifiers = aPerson.GetIdentifier(IdentifierType.NhsNumber);
            Assert.Single(identifiers);
            Assert.Equal(nhsNumberIdentifier, identifiers[0]);
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

        public static IndentityRepositoryBuilder AnIdentityRepository => new IndentityRepositoryBuilder();
        
        public class IndentityRepositoryBuilder
        {
            private Person[] people;

            public IndentityRepositoryBuilder WithPeople(params Person[] newPeople)
            {
                return Copy(with: @new => @new.people = (Person[])newPeople.Clone());
            }

            private IndentityRepositoryBuilder Copy(Action<IndentityRepositoryBuilder> with)
            {
                var copy = (IndentityRepositoryBuilder)MemberwiseClone();
                with(copy);
                return copy;
            }

            public static implicit operator IdentityRepository(IndentityRepositoryBuilder builder)
            {
                return builder.Build();
            }

            public IdentityRepository Build()
            {
                return new IdentityRepository(people.AsQueryable());
            }
        }


        public static Identifier BradfordHospitalNumber(string hosptialNumber) =>
            IdentifierType.BradfordHospitalNumber.Parse(hosptialNumber);

    }
    
    public class IdentityRepository
    {
        private readonly IQueryable<Person> people;

        public IdentityRepository(IQueryable<Person> people)
        {
            this.people = people;
        }

        public Person FindPersonBy(Identifier identifier)
        {
            if (Equals(identifier.Type, IdentifierType.NhsNumber))
            {
                var nhsNumber = ((IdentifierStringValue) identifier.Value).Value;
                return people.FirstOrDefault(_ => _.NhsNumber == nhsNumber);
            }

            if(Equals(identifier.Type, IdentifierType.BradfordHospitalNumber))
            {
                var hosptialNumber = ((IdentifierStringValue) identifier.Value).Value;
                return people.FirstOrDefault(
                    _ => _.BradfordHosptialNumbers.Contains(hosptialNumber));
            }

            throw new InvalidOperationException($"Don't handle querying '{identifier.Type.ExternalId}'");
        }
    }
}