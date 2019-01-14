using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;

namespace CHC.Consent.EFCore
{
    public class PersonIdentifierXmlMarshallers
    {
        public PersonIdentifierXmlMarshallers(IdentifierDefinitionRegistry registry)
        {
            Marshallers = registry.Accept(new IdentifierXmlMarshallerCreator()).Marshallers;
        }

        public IDictionary<string, IIdentifierXmlMarshaller> Marshallers { get; }

        public XElement MarshallToXml(PersonIdentifier identifier)
        {
            return Marshallers[identifier.Definition.SystemName].MarshallToXml(identifier);
        }

        public PersonIdentifier MarshallFromXml(PersonIdentifierEntity entity)
        {
            return MarshallFromXml(entity.TypeName, XElement.Parse(entity.Value));
        }

        public PersonIdentifier MarshallFromXml(string typeName, XElement xElement)
        {
            return Marshallers[typeName].MarshallFromXml(xElement);
        }

        public IEnumerable<PersonIdentifier> MarshallFromXml(IEnumerable<PersonIdentifierEntity> identifierEntities)
        {
            return identifierEntities
                .Select(this.MarshallFromXml);
        }
    }
}