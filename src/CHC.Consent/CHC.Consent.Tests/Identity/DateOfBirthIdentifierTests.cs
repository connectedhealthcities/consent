using System;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.IdentifierValues;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class DateOfBirthIdentifierTests
    {
        [Fact]
        public void ParsesDateCorrectly()
        {
            var value = new DateIdentifierValueType().Parse("2018-11-01");
            Assert.IsType<DateIdentifierValue>(value);
            Assert.Equal(new DateTime(2018, 11, 1), ((DateIdentifierValue) value).Value);
        }

        [Fact]
        public void CanUpdateAPersonWithNoDateOfBirth()
        {
            var dateOfBirth = Create.DateOfBirth(3.April(2017));
            var person = new Person();

            dateOfBirth.Update(person);
            
            Assert.Equal(3.April(2017), person.DateOfBirth);
        }

        [Fact]
        public void WillNotUpdateAPersonWithADateOfBirth()
        {
            var attemptedDateOfBirth = 3.April(2017);
            var originalDateOfBirth = 19.June(2001);
            
            var dateOfBirth = Create.DateOfBirth(attemptedDateOfBirth);
            var person = new Person { DateOfBirth = originalDateOfBirth};

            Assert.Throws<InvalidOperationException>(() => dateOfBirth.IdentifierType.Update(person, dateOfBirth.Value));
            
            Assert.Equal(originalDateOfBirth, person.DateOfBirth);
        }
        
        [Fact]
        public void CanUpdatePersonWithASameDateOfBirth()
        {
            var dateOfBirth = Create.DateOfBirth(3.April(2017));
            var person = new Person { DateOfBirth = 3.April(2017)};

            dateOfBirth.Update(person);
            
            Assert.Equal(3.April(2017), person.DateOfBirth);
        }
    }
}