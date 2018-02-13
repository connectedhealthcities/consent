using System;
using System.Linq.Expressions;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class IdentifierRegistryTests
    {
        private readonly IdentifierRegistry registry = new IdentifierRegistry();

        // ReSharper disable once ClassNeverInstantiated.Local
        private class NoIdentifierAttribute : IIdentifier
        {
            public Expression<Func<Person, bool>> GetMatchExpression() => throw new NotImplementedException();
            public void Update(Person person) => throw new NotImplementedException();
        }

        [Fact]
        public void WhenRegisteringAnIdentifierItPicksOutTheNameFromTheIdentifierAttribute()
        {
            registry.Add<NhsNumberIdentifier>();
            
            Assert.Equal(typeof(NhsNumberIdentifier), registry[NhsNumberIdentifier.TypeName]);
        }

        [Fact]
        public void ThrowsAnExceptionWhenRegisteringATypeWithNoAttribute()
        {
            Assert.Throws<ArgumentException>(() => registry.Add<NoIdentifierAttribute>());
        }
    }
}