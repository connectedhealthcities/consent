using CHC.Consent.Testing.Utils;
using FluentAssertions;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class PersonIdentifierTests
    {
        [Fact]
        public void SimpleValuesAreEqual()
        {
            var value = Random.String();
            Identifiers.NhsNumber(value).Should().Be(Identifiers.NhsNumber(value));
        }
        
        [Fact]
        public void SimpleValuesWithDifferentValuesAreNotEqual()
        {
            var value = Random.String();
            Identifiers.NhsNumber(value).Should().NotBe(Identifiers.NhsNumber(Random.String()));
        }

        [Fact]
        public void CompositeValuesAreEqual()
        {
            Identifiers.Name("One", "Two")
                .Should().Be(Identifiers.Name("One", "Two"));
        }
    }
}