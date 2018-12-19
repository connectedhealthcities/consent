using System.Xml.Linq;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.EFCore.Identity
{
    public class IdentifierToXmlElementMarshaller : IIdentifierMarshaller
    {
        public IdentifierDefinition Definition { get; }
        private IStringValueParser Parser { get; }

        /// <inheritdoc />
        public IdentifierToXmlElementMarshaller(IdentifierDefinition definition, IStringValueParser parser=null)
        {
            Definition = definition;
            Parser = parser ?? PassThroughParser.Instance;
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

        private class PassThroughParser : IStringValueParser
        {
            public static IStringValueParser Instance { get; } = new PassThroughParser();
            /// <inheritdoc />
            private PassThroughParser()
            {
            }

            /// <inheritdoc />
            public bool TryParse(string value, out object result)
            {
                result = value;
                return true;
            }
        }
    }
}