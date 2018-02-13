using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity.Identifiers;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class SexIdentifierTests
    {
        [Fact]
        public void CanFilterPeopleBySex()
        {
            var identifier = new SexIdentifier { Sex = Sex.Male };

            var allPeople = new[]
            {
                new Person {Sex = Sex.Female},
                new Person {Sex = Sex.Male},
                new Person {Sex = Sex.Unknown},
                new Person {Sex = Sex.Male},
                new Person {Sex = Sex.Female},
            }.AsQueryable();


            var matchExpression = identifier.GetMatchExpression();

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
        
        private static SexIdentifier CreateSexIdentifier(Sex sex)
        {
            return new SexIdentifier {Sex = sex};
        }
    }
}