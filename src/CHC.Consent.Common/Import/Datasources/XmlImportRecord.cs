using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Evidence;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Common.Import.Datasources
{
    public class XmlImportRecord : IImportRecord
    {
        public List<IdentityRecord> Identities { get; } = new List<IdentityRecord>();
        IReadOnlyList<IdentityRecord> IImportRecord.Identities => Identities; 

        public List<Evidence.Evidence> Evidence { get; } = new List<Evidence.Evidence>();
        IReadOnlyList<Evidence.Evidence> IImportRecord.Evidence => Evidence;
        public IReadOnlyList<Match.MatchRecord> MatchIdentity { get; set; }
        public IReadOnlyList<Match.MatchRecord> MatchStudyIdentity { get; set; }
    }
}