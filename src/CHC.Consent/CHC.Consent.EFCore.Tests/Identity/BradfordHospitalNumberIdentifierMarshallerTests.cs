using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Identity;
using Xunit;

namespace CHC.Consent.EFCore.Tests.Identity
{
    public class BradfordHospitalNumberIdentifierMarshallerTests
    {
        private readonly BradfordHospitalNumberIdentifierMarshaller marhsaller = new BradfordHospitalNumberIdentifierMarshaller();

        [Fact]
        public void MarshallsIdentifierToValueString()
        {
            var marshalledValue = marhsaller.MarshalledValue(new BradfordHospitalNumberIdentifier("A Value"));
            Assert.Equal("A Value", marshalledValue);
        }

        [Theory]
        [InlineData(BradfordHospitalNumberIdentifierMarshaller.ValueTypeName, null, false, null)]
        [InlineData(BradfordHospitalNumberIdentifierMarshaller.ValueTypeName, "value", true, "value")]
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