using System;

namespace CHC.Consent.Api.Features.Identity.Dto
{
    public class MatchSpecification
    {
        public MatchIdentifierSpecification[] Identifiers { get; set; } = Array.Empty<MatchIdentifierSpecification>();
        
    }
}