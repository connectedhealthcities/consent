using System;
using System.Collections.Generic;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class IdentifierIdentifierValueDtoMarshaller<TIdentifier, TDefinition> : IIdentifierDefinitionVisitor
        where TDefinition : DefinitionBase
        where TIdentifier:IIdentifier<TDefinition> 
    {
        public interface IMarshaller
        {
            IIdentifierValueDto MarshallToDto(TIdentifier identifier);
            TIdentifier MarshallToIdentifier(IIdentifierValueDto dto);
        }

        public IDictionary<string, IMarshaller> Marshallers { get; } = new Dictionary<string, IMarshaller>();
        public IdentifierDtoMarshaller<TIdentifier,TDefinition>.CreateIdentifier IdentifierCreator { get; }

        /// <inheritdoc />
        public IdentifierIdentifierValueDtoMarshaller(IdentifierDtoMarshaller<TIdentifier,TDefinition>.CreateIdentifier identifierCreator)
        {
            IdentifierCreator = identifierCreator;
        }


        private class SimpleMarshaller<TValue> : IMarshaller
        {
            public TDefinition Definition { get; }
            public IdentifierDtoMarshaller<TIdentifier,TDefinition>.CreateIdentifier CreateIdentifier { get; }

            /// <inheritdoc />
            public SimpleMarshaller(TDefinition definition, IdentifierDtoMarshaller<TIdentifier,TDefinition>.CreateIdentifier createIdentifier)
            {
                Definition = definition;
                CreateIdentifier = createIdentifier;
            }

            /// <inheritdoc />
            public IIdentifierValueDto MarshallToDto(TIdentifier identifier)
            {
                return
                    new IdentifierValueDto<TValue>(
                        identifier.Definition.SystemName, 
                        (TValue)identifier.Value.Value
                    );
            }

            /// <inheritdoc />
            public TIdentifier MarshallToIdentifier(IIdentifierValueDto dto)
            {
                return CreateIdentifier(Definition, new SimpleIdentifierValue(dto.Value));
            }
        }
        
        class CompositeMarshaller : IMarshaller
        {
            public TDefinition Definition { get; }
            public IdentifierDtoMarshaller<TIdentifier,TDefinition>.CreateIdentifier CreateIdentifier { get; }
            private readonly IdentifierDtoMarshaller<TIdentifier, TDefinition> identifierDtoMarshaller;

            /// <inheritdoc />
            public CompositeMarshaller(CompositeIdentifierType type, TDefinition definition, IdentifierDtoMarshaller<TIdentifier,TDefinition>.CreateIdentifier createIdentifier)
            {
                Definition = definition;
                CreateIdentifier = createIdentifier;
                identifierDtoMarshaller = new IdentifierDtoMarshaller<TIdentifier, TDefinition>(type.Identifiers, createIdentifier);
            }

            public IIdentifierValueDto MarshallToDto(TIdentifier identifier)
            {
                var values = ((CompositeIdentifierValue<TIdentifier>)identifier.Value).Identifiers;
                return
                    new IdentifierValueDto<IIdentifierValueDto[]>(
                        identifier.Definition.SystemName,
                        identifierDtoMarshaller.MarshallToDtos(values)
                    );
            }

            /// <inheritdoc />
            public TIdentifier MarshallToIdentifier(IIdentifierValueDto dto)
            {
                var value = (IIdentifierValueDto[]) dto.Value;
                var identifiers = identifierDtoMarshaller.ConvertToIdentifiers(value);
                
                return CreateIdentifier(Definition, new CompositeIdentifierValue<TIdentifier>(identifiers));
            }
        }
        
        private void UseSimpleMarshaller<T>(IDefinition definition)
            => UseMarshaller(definition, new SimpleMarshaller<T>((TDefinition)definition, IdentifierCreator));

        private void UseMarshaller(
            IDefinition definition, 
            IMarshaller marshaller)
        {
            Marshallers[definition.SystemName] = marshaller;
        }

        /// <inheritdoc />
        public void Visit(IDefinition definition, DateIdentifierType type)
        {
            UseSimpleMarshaller<DateTime>(definition);
        }

        /// <inheritdoc />
        public void Visit(IDefinition definition, EnumIdentifierType type)
        {
            UseSimpleMarshaller<string>(definition);
        }

        /// <inheritdoc />
        public void Visit(IDefinition definition, CompositeIdentifierType type)
        {
            UseMarshaller(definition, new CompositeMarshaller(type, (TDefinition) definition, IdentifierCreator));
        }

        /// <inheritdoc />
        public void Visit(IDefinition definition, IntegerIdentifierType type)
        {
            UseSimpleMarshaller<long>(definition);
        }

        /// <inheritdoc />
        public void Visit(IDefinition definition, StringIdentifierType type)
        {
            UseSimpleMarshaller<string>(definition);
        }
        
        
    }
}