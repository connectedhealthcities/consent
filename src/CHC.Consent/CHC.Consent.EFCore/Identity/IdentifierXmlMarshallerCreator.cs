using System;
using System.Collections.Generic;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.EFCore.Identity
{
    public class IdentifierXmlMarshallerCreator : IIdentifierDefinitionVisitor
    {
        public IDictionary<string, IIdentifierXmlMarshaller> Marshallers { get; }

        /// <inheritdoc />
        public IdentifierXmlMarshallerCreator()
        {
            Marshallers = new Dictionary<string, IIdentifierXmlMarshaller>();
        }


        private void SetMarshaller(IdentifierDefinition definition, IStringValueParser parser=null)
        {
            SetMarshaller(definition, new IdentifierXmlElementMarshaller(
                definition,
                parser ?? PassThroughParser.Instance));
        }

        private void SetMarshaller<T>(IdentifierDefinition definition, ValueParser<T>.TryParseDelegate tryParse)
        {
            SetMarshaller(definition, new ValueParser<T>(tryParse));
        }

        private void SetMarshaller(IdentifierDefinition definition, IIdentifierXmlMarshaller marshaller)
        {
            Marshallers[definition.SystemName] = marshaller;
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, DateIdentifierType type)
        {
            SetMarshaller<DateTime>(definition, DateTime.TryParse);
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, EnumIdentifierType type)
        {
            SetMarshaller(definition);
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, CompositeIdentifierType type)
        {
            SetMarshaller(definition, new CompositeIdentifierXmlMarshaller(definition));
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, IntegerIdentifierType type)
        {
            SetMarshaller<long>(definition, long.TryParse);
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, StringIdentifierType type)
        {
            SetMarshaller(definition);
        }
    }
}