using System;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class NhsNumberIdentityTests
    {
        private const string NhsNumber = "2134";
        private readonly Identifier nhsNumberIdentifier;

        public NhsNumberIdentityTests()
        {
            nhsNumberIdentifier = Create.NhsNumber(NhsNumber);
        }

        [Fact]
        public void CanUpdatePersonWithNoNhsNumber()
        {
            var person = new Person();
            
            nhsNumberIdentifier.Update(person);
            
            Assert.Equal(NhsNumber, person.NhsNumber);
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