using System;
using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using JetBrains.Annotations;

namespace CHC.Consent.EFCore.Identity
{
    public class IdentifierToXmlElementMarshaller : IIdentifierMarshaller
    {
        public IdentifierDefinition Definition { get; }
        private IStringValueParser Parser { get; }

        /// <inheritdoc />
        public IdentifierToXmlElementMarshaller(IdentifierDefinition definition) : this(definition, PassThroughParser.Instance)
        {
        }

        /// <inheritdoc />
        public IdentifierToXmlElementMarshaller([NotNull] IdentifierDefinition definition, [NotNull] IStringValueParser parser)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public XElement MarshallToXml(PersonIdentifier identifier)
        {
            return new XElement(Definition.SystemName, identifier.Value.Value);
        }

        public PersonIdentifier MarshallFromXml(XElement xElement)
        {
            Parser.TryParse(xElement.Value, out var value);
            return new PersonIdentifier(new IdentifierValue(value), Definition);
        }
    }
}