using System;
using System.Collections.Generic;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.EFCore.Identity
{
    public class IdentifierMarshallerCreator : IIdentifierDefinitionVisitor
    {
        private string currentKey;
        private IdentifierDefinition currentDefinition;
        public IDictionary<string, IIdentifierMarshaller> Marshallers { get; }

        /// <inheritdoc />
        public IdentifierMarshallerCreator(IDictionary<string, IIdentifierMarshaller> marshallers)
        {
            Marshallers = marshallers;
        }

        /// <inheritdoc />
        public void Visit(DateIdentifierType type)
        {
            Marshallers[currentKey] = new IdentifierToXmlElementMarshaller(currentDefinition, new ValueParser<DateTime>(DateTime.TryParse));
        }

        /// <inheritdoc />
        public void Visit(EnumIdentifierType type)
        {
            Marshallers[currentKey] = new IdentifierToXmlElementMarshaller(currentDefinition);
        }

        /// <inheritdoc />
        public void Visit(CompositeIdentifierType type)
        {
            Marshallers[currentKey] = new CompositeIdentifierMarshaller(currentDefinition);
        }

        /// <inheritdoc />
        public void Visit(IntegerIdentifierType type)
        {
            Marshallers[currentKey] = new IdentifierToXmlElementMarshaller(currentDefinition, new ValueParser<long>(long.TryParse));
        }

        /// <inheritdoc />
        public void Visit(StringIdentifierType type)
        {
            Marshallers[currentKey] = new IdentifierToXmlElementMarshaller(currentDefinition);
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition)
        {
            currentKey = definition.SystemName;
            currentDefinition = definition;
        }
    }
}