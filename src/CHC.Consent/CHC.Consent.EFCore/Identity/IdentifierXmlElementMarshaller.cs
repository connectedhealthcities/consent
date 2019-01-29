using System;
using System.Xml.Linq;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;
using JetBrains.Annotations;

namespace CHC.Consent.EFCore.Identity
{
    public class IdentifierXmlElementMarshaller<TIdentifier, TDefinition> :
        IIdentifierXmlMarhsaller<TIdentifier, TDefinition> 
        where TIdentifier : IIdentifier<TDefinition>, new()
        where TDefinition : DefinitionBase
    {
        public TDefinition Definition { get; }
        private IStringValueParser Parser { get; }

        /// <inheritdoc />
        public IdentifierXmlElementMarshaller(TDefinition definition) : this(definition, PassThroughParser.Instance)
        {
        }

        /// <inheritdoc />
        public IdentifierXmlElementMarshaller([NotNull] TDefinition definition, [NotNull] IStringValueParser parser)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public XElement MarshallToXml(TIdentifier identifier)
        {
            return new XElement(Definition.SystemName, identifier.Value.Value);
        }

        public TIdentifier MarshallFromXml(XElement xElement)
        {
            Parser.TryParse(xElement.Value, out var value);
            return new TIdentifier {Value = new SimpleIdentifierValue(value), Definition = Definition};
        }
    }
}