using System.Collections.Generic;
using CHC.Consent.Common.Import.Match;
using CHC.Consent.Import.Core;

namespace CHC.Consent.Common.Import.Datasources
{
    public class XmlImportRecord : IImportRecord
    {
        public List<IdentityRecord> Identities { get; } = new List<IdentityRecord>();
        IReadOnlyList<IdentityRecord> IImportRecord.Identities => Identities; 

        public List<EvidenceRecord> Evidence { get; } = new List<EvidenceRecord>();
        IReadOnlyList<EvidenceRecord> IImportRecord.Evidence => Evidence;
        
        public IReadOnlyList<MatchRecord> MatchIdentity { get; set; } = new MatchRecord[0];

        public IReadOnlyList<MatchByIdentityKindIdRecord> MatchStudyIdentity { get; set; } =
            new MatchByIdentityKindIdRecord[0];
    }
}