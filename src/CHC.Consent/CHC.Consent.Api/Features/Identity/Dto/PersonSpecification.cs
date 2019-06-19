 using System.Collections.Generic;
 using CHC.Consent.Api.Infrastructure;
 using CHC.Consent.Common.Identity;
 using CHC.Consent.Common.Identity.Identifiers;

 namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class PersonSpecification
    {
        public List<IIdentifierValueDto> Identifiers { get; set; } = new List<IIdentifierValueDto>();

        public UpdateMode UpdateMode { get; set; }
        
        public string Authority { get; set; }

        public List<MatchSpecification> MatchSpecifications { get; set; } = new List<MatchSpecification>();
    }
}