using System.Collections.Generic;
using CHC.Consent.Common.Core;
using CHC.Consent.Identity.Core;

namespace CHC.Consent.Common.Import
{
    public class PersonSpecification
    {
        public List<IIdentity> Identities { get; set; } = new List<IIdentity>();
        public List<IMatch> MatchIdentity { get; set; } = new List<IMatch>();
        public List<IIdentity> MatchSubjectIdentity { get; set; } = new List<IIdentity>();
        public IReadOnlyList<IEvidence> Evidence { get; set; } = new List<IEvidence>();
    }
}