using System;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore;
using FakeItEasy;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class IdentifierRegistryTests
    {
        private readonly PersonIdentifierRegistry registry = new PersonIdentifierRegistry();

        // ReSharper disable once ClassNeverInstantiated.Local
        private class NoIdentifierAttribute : IIdentifier
        {
        }

        [Fact]
        public void WhenRegisteringAnIdentifierItPicksOutTheNameFromTheIdentifierAttribute()
        {
            registry.Add<NhsNumberIdentifier, DummyAttributeAdapter<NhsNumberIdentifier>>();
            
            Assert.Equal(typeof(NhsNumberIdentifier), registry[NhsNumberIdentifier.TypeName]);
        }

        [Fact]
        public void ThrowsAnExceptionWhenRegisteringATypeWithNoAttribute()
        {
            Assert.Throws<ArgumentException>(() => registry.Add<NoIdentifierAttribute, DummyAttributeAdapter<NoIdentifierAttribute>>());
        }
    }
}