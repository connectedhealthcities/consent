using Xunit;

namespace CHC.Consent.Utils.Tests
{
    public class UnitTests
    {
        [Fact]
        public void UnitValueIsASingleton()
        {
            Assert.Same(Unit.Value, Unit.Value);
        }
    }
}