using System.Collections.Generic;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.Api.Features.Identity
{
    public class AgencyInfoDto
    {
        public Agency Agency { get; set; }
        public IEnumerable<IdentifierDefinition> Identifiers { get; set; }
    }
}