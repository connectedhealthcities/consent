using System;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using FakeItEasy;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class IdentifierRegistryTests
    {
        private readonly TypeRegistry<IIdentifier, IdentifierAttribute> registry = new TypeRegistry<IIdentifier, IdentifierAttribute>();

        // ReSharper disable once ClassNeverInstantiated.Local
        private class NoIdentifierAttribute : IIdentifier
        {
        }

        [Fact]
        public void WhenRegisteringAnIdentifierItPicksOutTheNameFromTheIdentifierAttribute()
        {
            registry.Add(typeof(NhsNumberIdentifier), IdentifierAttribute.GetAttribute(typeof(NhsNumberIdentifier)));
            
            Assert.Equal(typeof(NhsNumberIdentifier), registry[NhsNumberIdentifier.TypeName]);
        }

        [Fact]
        public void ThrowsAnExceptionWhenRegisteringATypeWithNoAttribute()
        {
            Assert.Throws<ArgumentException>(() => IdentifierAttribute.GetAttribute(typeof(NoIdentifierAttribute)));
        }
    }
}