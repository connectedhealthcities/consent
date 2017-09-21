using System.Collections.Generic;
using CHC.Consent.Common.Evidence;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Common.Import.Datasources
{
    public class XmlPerson : IPerson
    {
        public IDictionary<IdentityKind, Identity.Identity> Identities { get; } = new Dictionary<IdentityKind, Identity.Identity>();
        public IDictionary<EvidenceKind, Evidence.Evidence> Evidence { get; } = new Dictionary<EvidenceKind, Evidence.Evidence>();
        public IReadOnlyList<Match.Match> MatchIdentity { get; set; }
        public IReadOnlyList<Match.Match> MatchStudyIdentity { get; set; }
    }
}