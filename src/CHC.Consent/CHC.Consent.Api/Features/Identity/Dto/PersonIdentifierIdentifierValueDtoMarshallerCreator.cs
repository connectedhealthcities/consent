using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using IdentityModel.Internal;
using Newtonsoft.Json.Linq;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class PersonIdentifierIdentifierValueDtoMarshallerCreator : IIdentifierDefinitionVisitor
    {
        public IDictionary<string, PersonIdentifiersDtosMarshaller.IMarshaller> Marshallers { get; }
            = new Dictionary<string, PersonIdentifiersDtosMarshaller.IMarshaller>();

        private class SimpleMarshaller<T> : PersonIdentifiersDtosMarshaller.IMarshaller
        {
            public IdentifierDefinition Definition { get; }

            /// <inheritdoc />
            public SimpleMarshaller(IdentifierDefinition definition)
            {
                Definition = definition;
            }

            /// <inheritdoc />
            public IIdentifierValueDto MarshallToDto(PersonIdentifier identifier)
            {
                return
                    new IdentifierValueDto<T>(
                        identifier.Definition.SystemName, 
                        (T)identifier.Value.Value
                    );
            }

            /// <inheritdoc />
            public PersonIdentifier MarshallToIdentifier(IIdentifierValueDto dto)
            {
                return new PersonIdentifier(new SimpleIdentifierValue(dto.Value), Definition);
            }
        }

        class CompositeMarshaller : PersonIdentifiersDtosMarshaller.IMarshaller
        {
            public IdentifierDefinition Definition { get; }
            private readonly PersonIdentifiersDtosMarshaller marshaller;

            /// <inheritdoc />
            public CompositeMarshaller(CompositeIdentifierType type, IdentifierDefinition definition)
            {
                Definition = definition;
                marshaller = new PersonIdentifiersDtosMarshaller(type.Identifiers);
            }

            public IIdentifierValueDto MarshallToDto(PersonIdentifier identifier)
            {
                var values = ((CompositeIdentifierValue)identifier.Value).Identifiers;
                return
                    new IdentifierValueDto<IIdentifierValueDto[]>
                    (
                        identifier.Definition.SystemName,
                        marshaller.MarshallToDtos(values)
                    );
            }

            /// <inheritdoc />
            public PersonIdentifier MarshallToIdentifier(IIdentifierValueDto dto)
            {
                var value = (IIdentifierValueDto[]) dto.Value;
                var identifiers = marshaller.ConvertToIdentifiers(value);
                
                return new PersonIdentifier(new CompositeIdentifierValue(identifiers), Definition);
            }
        }

        private void UseSimpleMarshaller<T>(IdentifierDefinition definition)
            => UseMarshaller(definition, new SimpleMarshaller<T>(definition));

        private PersonIdentifiersDtosMarshaller.IMarshaller UseMarshaller(IdentifierDefinition definition, PersonIdentifiersDtosMarshaller.IMarshaller marshaller)
        {
            return Marshallers[definition.SystemName] = marshaller;
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, DateIdentifierType type)
        {
            UseSimpleMarshaller<DateTime>(definition);
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, EnumIdentifierType type)
        {
            UseSimpleMarshaller<string>(definition);
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, CompositeIdentifierType type)
        {
            UseMarshaller(definition, new CompositeMarshaller(type, definition));
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, IntegerIdentifierType type)
        {
            UseSimpleMarshaller<long>(definition);
        }

        /// <inheritdoc />
        public void Visit(IdentifierDefinition definition, StringIdentifierType type)
        {
            UseSimpleMarshaller<string>(definition);
        }
    }

    
}