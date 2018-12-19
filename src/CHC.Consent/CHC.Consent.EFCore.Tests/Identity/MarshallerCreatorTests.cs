using System.Collections.Generic;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Identity;
using Xunit;

namespace CHC.Consent.EFCore.Tests.Identity
{
    public class MarshallerCreatorTests 
    {
        public static IEnumerable<object[]> StringMarshalledIdentifierTypes()
        {
            yield return new object[] {new DateIdentifierType()};
            yield return new object[] {new EnumIdentifierType() };
            yield return new object[] {new IntegerIdentifierType() };
            yield return new object[] {new StringIdentifierType() };
        }
        
        [Theory]
        [MemberData(nameof(StringMarshalledIdentifierTypes))]
        public void CreatesStringIdentifierMarshaller(IIdentifierType type)
        {
            var identifierDefinition = new IdentifierDefinition("test", type);

            var creator = new IdentifierMarshallerCreator(new Dictionary<string, IIdentifierMarshaller>());
            
            identifierDefinition.Accept(creator);
            
            Assert.Contains("test", creator.Marshallers.Keys);
            
            var marshaller = creator.Marshallers["test"];

            var stringMarshaller = Assert.IsType<IdentifierToXmlElementMarshaller>(marshaller);
            Assert.Equal(identifierDefinition, stringMarshaller.Definition);
        }
    }
}