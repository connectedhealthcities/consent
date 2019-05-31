using System.Collections.Generic;
using CHC.Consent.Api.Infrastructure;

namespace CHC.Consent.Api.Features.Identity
{
    public class AgencyPersonDto
    {
        public string Id { get; }
        public IEnumerable<IIdentifierValueDto> IdentifierValueDtos { get; }

        public AgencyPersonDto(string id, IEnumerable<IIdentifierValueDto> identifierValueDtos)
        {
            Id = id;
            IdentifierValueDtos = identifierValueDtos;
        }
    }
}