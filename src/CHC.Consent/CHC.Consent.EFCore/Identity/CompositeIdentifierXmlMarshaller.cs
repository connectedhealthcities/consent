using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.EFCore.Identity
{
    //TODO: Add logging and handling of unknown identifiers
    public class CompositeIdentifierXmlMarshaller : IIdentifierXmlMarshaller
    {
        private readonly CompositeIdentifierType IdentifierType;
        public IdentifierDefinition Definition { get; }
        public PersonIdentifierXmlMarshallers Marshallers { get; }

        public CompositeIdentifierXmlMarshaller(IdentifierDefinition definition)
        {
            Definition = definition;
            IdentifierType = (CompositeIdentifierType)Definition.Type;
            Marshallers = new PersonIdentifierXmlMarshallers(IdentifierType.Identifiers);
        }

        /// <inheritdoc />
        public XElement MarshallToXml(PersonIdentifier identifier)
        {
            var values = ((IEnumerable<PersonIdentifier>) identifier.Value.Value).ToDictionary(_ => _.Definition.SystemName);
            return new XElement(
                Definition.SystemName,
                IdentifierType.Identifiers
                    .Where(id => values.ContainsKey(id.SystemName))
                    .Select(id => Marshallers.MarshallToXml(values[id.SystemName])));

        }

        /// <inheritdoc />
        public PersonIdentifier MarshallFromXml(XElement xElement)
        {
            return new PersonIdentifier(
                new IdentifierValue(
                    IdentifierType.Identifiers
                        .Select(id => ( typeName: id.SystemName, value: xElement.Element(id.SystemName)))
                        .Where(i => i.value != null)
                        .Select(i => Marshallers.MarshallFromXml(i.typeName, i.value))
                        .ToArray()),
                Definition);
        }
    }
}