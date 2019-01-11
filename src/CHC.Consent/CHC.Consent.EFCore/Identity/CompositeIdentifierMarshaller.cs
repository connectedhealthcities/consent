using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.EFCore.Identity
{
    public class CompositeIdentifierMarshaller : IIdentifierMarshaller
    {
        private readonly CompositeIdentifierType IdentifierType;
        public IdentifierDefinition Definition { get; }
        public IDictionary<string, IIdentifierMarshaller> Marshallers { get; } = new Dictionary<string, IIdentifierMarshaller>();

        public CompositeIdentifierMarshaller(IdentifierDefinition definition)
        {
            Definition = definition;
            IdentifierType = (CompositeIdentifierType)Definition.Type;
            CreateMarshallersForDefinition();
        }

        private void CreateMarshallersForDefinition()
        {
            IdentifierType.Identifiers.Accept(new IdentifierMarshallerCreator(Marshallers));
        }

        /// <inheritdoc />
        public XElement MarshallToXml(PersonIdentifier identifier)
        {
            var values = ((IEnumerable<PersonIdentifier>) identifier.Value.Value).ToDictionary(_ => _.Definition.SystemName);
            return new XElement(
                Definition.SystemName,
                IdentifierType.Identifiers.Cast<KeyValuePair<string, IdentifierDefinition>>()
                    .Where(id => values.ContainsKey(id.Key))
                    .Select(id => Marshallers[id.Key].MarshallToXml(values[id.Key])));

        }

        /// <inheritdoc />
        public PersonIdentifier MarshallFromXml(XElement xElement)
        {
            return new PersonIdentifier(
                new IdentifierValue(
                    IdentifierType.Identifiers.Cast<KeyValuePair<string, IdentifierDefinition>>()
                        .Select(id => (id: id, value: xElement.Element(id.Key)))
                        .Where(i => i.value != null)
                        .Select(i => Marshallers[i.id.Key].MarshallFromXml(i.value))
                        .ToArray()),
                Definition);
        }
    }
}