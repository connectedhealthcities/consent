using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using IdentityModel.Internal;
using Newtonsoft.Json.Linq;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class PersonIdentifierIdentifierValueDtoMarshallerCreator : IIdentifierDefinitionVisitor
    {
        public IDictionary<string, IMarshaller> Marshallers { get; } = new Dictionary<string,IMarshaller>();

        public interface IMarshaller
        {
            IIdentifierValueDto MarshallToDto(PersonIdentifier identifier);
            PersonIdentifier MarshallToIdentifier(IIdentifierValueDto dto);
        }

        private class SimpleMarshaller<T> : IMarshaller
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
                return new PersonIdentifier(new IdentifierValue(dto.Value), Definition);
            }
        }

        class CompositeMarshaller : IMarshaller
        {
            public IdentifierDefinition Definition { get; }
            private IDictionary<string, IMarshaller> marshallers;

            /// <inheritdoc />
            public CompositeMarshaller(CompositeIdentifierType type, IdentifierDefinition definition)
            {
                Definition = definition;
                var marshallerCreator = new PersonIdentifierIdentifierValueDtoMarshallerCreator();
                type.Identifiers.Accept(marshallerCreator);
                marshallers = marshallerCreator.Marshallers;
            }

            public IIdentifierValueDto MarshallToDto(PersonIdentifier identifier)
            {
                var values = (IEnumerable<PersonIdentifier>)identifier.Value.Value;
                var dtos = values.Select(i => marshallers[i.Definition.SystemName].MarshallToDto(i)).ToArray();
                return
                    new IdentifierValueDto<IIdentifierValueDto[]>
                    (
                        identifier.Definition.SystemName,
                        dtos
                    );
            }

            /// <inheritdoc />
            public PersonIdentifier MarshallToIdentifier(IIdentifierValueDto dto)
            {
                var value = (IIdentifierValueDto[]) dto.Value;
                var identifiers =
                    value
                        .Select(d => marshallers[d.DefinitionSystemName].MarshallToIdentifier(d))
                        .ToArray(); 
                
                return new PersonIdentifier(new IdentifierValue(identifiers), Definition);
            }

            private PersonIdentifier ConvertDto(IIdentifierValueDto dto)
            {
                return marshallers[dto.DefinitionSystemName].MarshallToIdentifier(dto);
            }
        }

        private void UseSimpleMarshaller<T>(IdentifierDefinition definition)
            => UseMarshaller(definition, new SimpleMarshaller<T>(definition));

        private IMarshaller UseMarshaller(IdentifierDefinition definition, IMarshaller marshaller)
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