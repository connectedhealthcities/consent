 using System.Collections.Generic;
 using CHC.Consent.Common.Identity;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class PersonSpecification
    {
        public List<IPersonIdentifier> Identifiers { get; set; } = new List<IPersonIdentifier>();

        public List<MatchSpecification> MatchSpecifications { get; set; } = new List<MatchSpecification>();
    }
}