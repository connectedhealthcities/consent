using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class NhsNumberIdentifierTests
    {
        private const string NhsNumber = "2134";
        private readonly IIdentifier nhsNumberIdentifier;

        public NhsNumberIdentifierTests()
        {
            nhsNumberIdentifier = new NhsNumberIdentifier(NhsNumber);
        }
        
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
        public void CanUpdatePersonWithNoNhsNumber()
        {
            var person = new Person();
            
            nhsNumberIdentifier.Update(person);
            
            Assert.Equal(NhsNumber, person.NhsNumber);
        }
        
        
        [Fact]
        public void CanFilterPeopleByDateOfBirth()
        {
            var identifier = nhsNumberIdentifier;

            var allPeople = new[]
            {
                new Person { NhsNumber = "Ignored"},
                new Person { NhsNumber = "Peter Crowther"},
                new Person { NhsNumber = "Orange tips"},
                new Person { NhsNumber = NhsNumber},
                new Person { NhsNumber= "Blah Blah Blah"},
            }.AsQueryable();


            var matchExpression = identifier.GetMatchExpression();

            Assert.All(
                allPeople.Where(matchExpression),
                _ => Assert.Equal(NhsNumber, _.NhsNumber));
        }
        
        
        [Fact]
        public void DoesNothingIfPersonAlreadyHasNhsNumber()
        {
            var person = new Person { NhsNumber = NhsNumber };
            
            nhsNumberIdentifier.Update(person);
            
            Assert.Equal(NhsNumber, person.NhsNumber);
        }
        
        [Fact]
        public void DoesNotAllowChangingNhsNumber()
        {
            var person = new Person { NhsNumber = "NOT THE SAME" };

            Assert.Throws<InvalidOperationException>(() => nhsNumberIdentifier.Update(person));
            
            Assert.Equal("NOT THE SAME", person.NhsNumber);
        }
    }
}