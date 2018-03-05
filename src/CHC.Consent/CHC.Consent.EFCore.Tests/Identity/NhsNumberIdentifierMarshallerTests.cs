using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Identity;
using Xunit;

namespace CHC.Consent.EFCore.Tests.Identity
{
    public class NhsNumberIdentifierMarshallerTests
    {
        private readonly NhsNumberIdentifierMarshaller marhsaller = new NhsNumberIdentifierMarshaller();

        [Fact]
        public void MarshallsIdentifierToValueString()
        {
            var marshalledValue = marhsaller.MarshalledValue(new NhsNumberIdentifier("A Value"));
            Assert.Equal("A Value", marshalledValue);
        }

        [Theory]
        [InlineData(NhsNumberIdentifierMarshaller.ValueTypeName, null, false, null)]
        [InlineData(NhsNumberIdentifierMarshaller.ValueTypeName, "value", true, "value")]
        [InlineData("Wrong Type", null, false, null)]
        [InlineData("Wrong Type", "value", false, null)]
        public void TestUnmarshalling(string valueType, string marshalledValue, bool expectIdentifier, string expectedValue)
        {
            var identifier = marhsaller.Unmarshall(valueType, marshalledValue);

            if (expectIdentifier)
            {
                Assert.NotNull(identifier);
            }
            else
            {
                Assert.Null(identifier);
            }
            
            Assert.Equal(expectedValue, identifier?.Value);
            
        }
            
        
    }
}