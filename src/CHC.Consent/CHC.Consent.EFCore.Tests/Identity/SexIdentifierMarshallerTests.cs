using CHC.Consent.Common;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Identity;
using Xunit;

namespace CHC.Consent.EFCore.Tests.Identity
{
    public class SexIdentifierMarshallerTests
    {
        private readonly SexIdentifierMarshaller marhsaller = new SexIdentifierMarshaller();

        [Theory]
        [InlineData(Sex.Male, "Male")]
        [InlineData(Sex.Female, "Female")]
        public void MarshallsIdentifierToValueString(Sex sex, string expectedValue)
        {
            var marshalledValue = marhsaller.MarshalledValue(new SexIdentifier(sex));
            Assert.Equal(expectedValue, marshalledValue);
        }

        [Theory]
        [InlineData(SexIdentifierMarshaller.ValueTypeName, null, false, null)]
        [InlineData(SexIdentifierMarshaller.ValueTypeName, "fish", false, null)]
        [InlineData(SexIdentifierMarshaller.ValueTypeName, "Male", true, Sex.Male)]
        [InlineData(SexIdentifierMarshaller.ValueTypeName, "Female", true, Sex.Female)]
        [InlineData("Wrong Type", null, false, null)]
        [InlineData("Wrong Type", "value", false, null)]
        [InlineData("Wrong Type", "male", false, null)]
        public void TestUnmarshalling(string valueType, string marshalledValue, bool createsIdentifier, Sex? expectedValue)
        {
            var identifier = marhsaller.Unmarshall(valueType, marshalledValue);

            if (createsIdentifier)
            {
                Assert.NotNull(identifier);
            }
            else
            {
                Assert.Null(identifier);
            }
            
            Assert.Equal(expectedValue, identifier?.Sex);
            
        }
            
        
    }
}