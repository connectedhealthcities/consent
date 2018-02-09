 using System.Collections.Generic;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class PersonSpecification
    {
        public List<IdentifierSpecification> Identifiers { get; set; } = new List<IdentifierSpecification>();

        public List<MatchSpecification> MatchSpecifications { get; set; } = new List<MatchSpecification>();
    }
}