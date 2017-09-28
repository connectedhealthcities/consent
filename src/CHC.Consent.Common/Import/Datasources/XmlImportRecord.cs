using System.Collections.Generic;
using CHC.Consent.Common.Import.Match;

namespace CHC.Consent.Common.Import.Datasources
{
    public class XmlImportRecord : IImportRecord
    {
        public List<IdentityRecord> Identities { get; } = new List<IdentityRecord>();
        IReadOnlyList<IdentityRecord> IImportRecord.Identities => Identities; 

        public List<Evidence.Evidence> Evidence { get; } = new List<Evidence.Evidence>();
        IReadOnlyList<Evidence.Evidence> IImportRecord.Evidence => Evidence;
        public IReadOnlyList<MatchRecord> MatchIdentity { get; set; } = new MatchRecord[0];

        public IReadOnlyList<MatchByIdentityKindIdRecord> MatchStudyIdentity { get; set; } =
            new MatchByIdentityKindIdRecord[0];
    }
}