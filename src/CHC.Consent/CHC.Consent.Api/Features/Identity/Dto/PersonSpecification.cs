 using System.Collections.Generic;
 using CHC.Consent.Common.Identity;
 using CHC.Consent.Common.Identity.Identifiers;

 namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class PersonSpecification
    {
        public List<IdentifierValueDto> Identifiers { get; set; } = new List<IdentifierValueDto>();

        public List<MatchSpecification> MatchSpecifications { get; set; } = new List<MatchSpecification>();
    }

    public class IdentifierValueDto
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}