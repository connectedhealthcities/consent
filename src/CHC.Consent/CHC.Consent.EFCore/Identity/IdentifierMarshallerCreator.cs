using System;
using System.Collections.Generic;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.EFCore.Identity
{
    public class IdentifierMarshallerCreator : IIdentifierDefinitionVisitor
    {
        public IDictionary<string, IIdentifierMarshaller> Marshallers { get; }

        /// <inheritdoc />
        public IdentifierMarshallerCreator(IDictionary<string, IIdentifierMarshaller> marshallers=null)
        {
            Marshallers = marshallers??new Dictionary<string, IIdentifierMarshaller>();
        }


        private void SetMarshaller(IdentifierDefinition definition, IStringValueParser parser=null)
        {
            SetMarshaller(definition, new IdentifierToXmlElementMarshaller(
                definition,
                parser ?? PassThroughParser.Instance));
        }

        private void SetMarshaller(IdentifierDefinition definition, IIdentifierMarshaller marshaller)
        {
            Marshallers[definition.SystemName] = marshaller;
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, DateIdentifierType type)
        {
            SetMarshaller(definition, new ValueParser<DateTime>(DateTime.TryParse));
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, EnumIdentifierType type)
        {
            SetMarshaller(definition);
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, CompositeIdentifierType type)
        {
            SetMarshaller(definition, new CompositeIdentifierMarshaller(definition));
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, IntegerIdentifierType type)
        {
            SetMarshaller(definition, new ValueParser<long>(long.TryParse));
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, StringIdentifierType type)
        {
            SetMarshaller(definition);
        }
    }
}