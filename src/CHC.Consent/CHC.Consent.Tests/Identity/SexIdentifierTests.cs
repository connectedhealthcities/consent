using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.IdentifierTypes;
using CHC.Consent.Common.Identity.IdentifierValues;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class SexIdentifierTests
    {
        private SexIdentifierType IdentifierType { get; }= new SexIdentifierType();

        [Fact]
        public void CanParseSex()
        {
            var value = IdentifierType.Parse("Male").Value;
            Assert.IsType<SexIdentifierValue>(value);
            Assert.Equal(Sex.Male, ((SexIdentifierValue)value).Sex);
        }

        [Fact]
        public void CannotParseUnknownSex()
        {
            Assert.Throws<FormatException>(() => IdentifierType.Parse("Peter Crowther"));
        }

        [Fact]
        public void CanFilterPeopleBySex()
        {
            var identifier = IdentifierType.Parse("Male");

            var allPeople = new[]
            {
                new Person {Sex = Sex.Female},
                new Person {Sex = Sex.Male},
                new Person {Sex = Sex.Unknown},
                new Person {Sex = Sex.Male},
                new Person {Sex = Sex.Female},
            }.AsQueryable();


            var matchExpression = identifier.IdentifierType.GetMatchExpression(identifier.Value);

            Assert.All(
                allPeople.Where(matchExpression),
                _ => Assert.Equal(Sex.Male, _.Sex));
        }

        [Fact]
        public void CanUpdatePersonSexWhenNoSexIsSpefied()
        {
            var identifier = CreateSexIdentifier(Sex.Female);
            var person = new Person();
            
            identifier.Update(person);

            Assert.Equal(Sex.Female, person.Sex);
        }
        
        [Fact]
        public void CanNotUpdatePersonWithExistingSex()
        {
            var identifier = CreateSexIdentifier(Sex.Female);
            var person = new Person { Sex = Sex.Male };
            
            Assert.Throws<InvalidOperationException>(() => identifier.Update(person));

            Assert.Equal(Sex.Male, person.Sex);
        }

        [Fact]
        public void CanUpdatePersonSexWhenSexIsUnchanged()
        {
            var identifier = CreateSexIdentifier(Sex.Female);
            var person = new Person { Sex = Sex.Female };
            
            identifier.Update(person);

            Assert.Equal(Sex.Female, person.Sex);
        }
        
        private Identifier CreateSexIdentifier(Sex sex)
        {
            return new Identifier(IdentifierType, IdentifierType.ValueType, new SexIdentifierValue(sex));
        }
    }
}