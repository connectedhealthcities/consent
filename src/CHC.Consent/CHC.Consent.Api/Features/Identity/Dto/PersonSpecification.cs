 using System.Collections.Generic;
 using CHC.Consent.Common.Identity;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class PersonSpecification
    {
        public List<IIdentifier> Identifiers { get; set; } = new List<IIdentifier>();

        public List<MatchSpecification> MatchSpecifications { get; set; } = new List<MatchSpecification>();
    }
}