using System.Text;
using CHC.Consent.Common.Identity.Identifiers.Medway;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.Testing.Utils;
using Xunit;

namespace CHC.Consent.EFCore.Tests.Identity
{
    public class XmlMarshallerTests
    {
        readonly XmlIdentifierMarshaller<MedwayNameIdentifier> marshaller = new XmlIdentifierMarshaller<MedwayNameIdentifier>(valueType: "BIB4All.MedwayName");
        
        [Fact]
        public void MarshallsNames()
        {
            var marshalledValue = marshaller.MarshalledValue(new MedwayNameIdentifier("Anthony", "Roberts"));

            Assert.Equal(
                Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()) + @"<MedwayNameIdentifier><FirstName>Anthony</FirstName><LastName>Roberts</LastName></MedwayNameIdentifier>",
                marshalledValue);
        }

        [Fact]
        public void RoundTripsAValue()
        {
            var originalValue = new MedwayNameIdentifier(Random.String(), Random.String());
            var marshalledValue = marshaller.MarshalledValue(originalValue);
            var unmarshalledValue = marshaller.Unmarshall(marshaller.ValueType, marshalledValue);
            
            Assert.Equal(originalValue.FirstName, unmarshalledValue.FirstName);
            Assert.Equal(originalValue.LastName, unmarshalledValue.LastName);
        }
    }
}