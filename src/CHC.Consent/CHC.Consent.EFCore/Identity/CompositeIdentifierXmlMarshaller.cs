using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.EFCore.Identity
{
    //TODO: Add logging and handling of unknown identifiers
    public class CompositeIdentifierXmlMarshaller<TIdentifier, TDefinition> :
        IIdentifierXmlMarhsaller<TIdentifier, TDefinition> 
        where TIdentifier : IIdentifier<TDefinition>, new()
        where TDefinition : DefinitionBase
        
    {
        private readonly CompositeIdentifierType IdentifierType;
        public TDefinition Definition { get; }
        public IdentifierXmlMarshallers<TIdentifier, TDefinition> Marshallers { get; }

        public CompositeIdentifierXmlMarshaller(TDefinition definition)
        {
            Definition = definition;
            IdentifierType = (CompositeIdentifierType)Definition.Type;
            Marshallers = new IdentifierXmlMarshallers<TIdentifier, TDefinition>(IdentifierType.Identifiers);
        }

        /// <inheritdoc />
        public XElement MarshallToXml(TIdentifier identifier)
        {
            var values = ((CompositeIdentifierValue<TIdentifier>) identifier.Value).Identifiers
                .ToDictionary(_ => _.Definition.SystemName);
            return new XElement(
                Definition.SystemName,
                IdentifierType.Identifiers
                    .Where(id => values.ContainsKey(id.SystemName))
                    .Select(id => Marshallers.MarshallToXml(values[id.SystemName])));

        }

        /// <inheritdoc />
        public TIdentifier MarshallFromXml(XElement xElement)
        {
            return new TIdentifier
            {
                Definition = Definition,
                Value = new CompositeIdentifierValue<TIdentifier>(
                    IdentifierType.Identifiers
                        .Select(id => (typeName: id.SystemName, value: xElement.Element(id.SystemName)))
                        .Where(i => i.value != null)
                        .Select(i => Marshallers.MarshallFromXml(i.typeName, i.value))
                        .ToArray())
            };


        }
    }
}