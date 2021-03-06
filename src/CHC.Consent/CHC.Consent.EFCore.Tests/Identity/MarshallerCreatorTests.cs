using System.Collections.Generic;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.Common.Infrastructure.Definitions.Types;
using CHC.Consent.EFCore.Identity;
using Xunit;

namespace CHC.Consent.EFCore.Tests.Identity
{
    public class MarshallerCreatorTests 
    {
        public static IEnumerable<object[]> StringMarshalledIdentifierTypes()
        {
            yield return new object[] {new DateDefinitionType()};
            yield return new object[] {new EnumDefinitionType() };
            yield return new object[] {new IntegerDefinitionType() };
            yield return new object[] {new StringDefinitionType() };
        }
        
        [Theory]
        [MemberData(nameof(StringMarshalledIdentifierTypes))]
        public void CreatesStringIdentifierMarshaller(IDefinitionType type)
        {
            var identifierDefinition = new IdentifierDefinition("test", type);

            var creator = new IdentifierXmlMarshallerCreator<PersonIdentifier, IdentifierDefinition>();
            
            identifierDefinition.Accept(creator);
            
            Assert.Contains("test", creator.Marshallers.Keys);
            
            var marshaller = creator.Marshallers["test"];

            var stringMarshaller = Assert.IsType<IdentifierXmlElementMarshaller<PersonIdentifier, IdentifierDefinition>>(marshaller);
            Assert.Equal(identifierDefinition, stringMarshaller.Definition);
        }
    }
}