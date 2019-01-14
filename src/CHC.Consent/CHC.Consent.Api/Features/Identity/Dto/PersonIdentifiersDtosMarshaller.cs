using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class PersonIdentifiersDtosMarshaller
    {
        private IDictionary<string, IMarshaller> Marshallers { get;  }

        public interface IMarshaller
        {
            IIdentifierValueDto MarshallToDto(PersonIdentifier identifier);
            PersonIdentifier MarshallToIdentifier(IIdentifierValueDto dto);
        }

        public PersonIdentifiersDtosMarshaller(IdentifierDefinitionRegistry registry)
        {
            Marshallers = registry.Accept<PersonIdentifierIdentifierValueDtoMarshallerCreator>().Marshallers; 
        }

        public IIdentifierValueDto[] MarshallToDtos(IEnumerable<PersonIdentifier> identifiers)
        {
            return identifiers.Select(MarshallToDto).ToArray();
        }

        private IIdentifierValueDto MarshallToDto(PersonIdentifier identifier)
        {
            return Marshallers[identifier.Definition.SystemName].MarshallToDto(identifier);
        }

        public PersonIdentifier[] ConvertToIdentifiers(IEnumerable<IIdentifierValueDto> dtos)
        {
            return dtos.Select(MarshallToIdentifier).ToArray();
        }

        private PersonIdentifier MarshallToIdentifier(IIdentifierValueDto dto)
        {
            return Marshallers[dto.DefinitionSystemName].MarshallToIdentifier(dto);
        }
    }
}