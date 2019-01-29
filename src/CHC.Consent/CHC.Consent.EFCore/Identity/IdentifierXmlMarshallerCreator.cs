using System;
using System.Collections.Generic;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Definitions;
using CHC.Consent.Common.Infrastructure.Definitions.Types;


namespace CHC.Consent.EFCore.Identity
{
    
    public class IdentifierXmlMarshallerCreator<TIdentifier, TDefinition> : IDefinitionVisitor
        where TIdentifier : IIdentifier<TDefinition>, new()
        where TDefinition : DefinitionBase, IDefinition
    {
        public IDictionary<string, IIdentifierXmlMarhsaller<TIdentifier, TDefinition>> Marshallers { get; }

        /// <inheritdoc />
        public IdentifierXmlMarshallerCreator()
        {
            Marshallers = new Dictionary<string, IIdentifierXmlMarhsaller<TIdentifier, TDefinition>>();
        }


        private void SetMarshaller(IDefinition definition, IStringValueParser parser=null)
        {
            SetMarshaller(definition, new IdentifierXmlElementMarshaller<TIdentifier, TDefinition>(
                (TDefinition)definition,
                parser ?? PassThroughParser.Instance));
        }

        private void SetMarshaller<T>(IDefinition definition, ValueParser<T>.TryParseDelegate tryParse)
        {
            SetMarshaller(definition, new ValueParser<T>(tryParse));
        }

        private void SetMarshaller(IDefinition definition, IIdentifierXmlMarhsaller<TIdentifier, TDefinition> marshaller)
        {
            Marshallers[definition.SystemName] = marshaller;
        }

        /// <inheritdoc />
        public void Visit(IDefinition definition, DateDefinitionType type)
        {
            SetMarshaller<DateTime>(definition, DateTime.TryParse);
        }

        /// <inheritdoc />
        public void Visit(IDefinition definition, EnumDefinitionType type)
        {
            SetMarshaller(definition);
        }

        /// <inheritdoc />
        public void Visit(IDefinition definition, CompositeDefinitionType type)
        {
            SetMarshaller(definition, new CompositeIdentifierXmlMarshaller<TIdentifier, TDefinition>((TDefinition)definition));
        }

        /// <inheritdoc />
        public void Visit(IDefinition definition, IntegerDefinitionType type)
        {
            SetMarshaller<long>(definition, long.TryParse);
        }

        /// <inheritdoc />
        public void Visit(IDefinition definition, StringDefinitionType type)
        {
            SetMarshaller(definition);
        }
    }
}