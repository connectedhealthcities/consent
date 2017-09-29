using System.Collections.Generic;
using CHC.Consent.Common.Import.Datasources;

namespace CHC.Consent.Common.Import
{
    public interface IImportRecord
    {
        IReadOnlyList<IdentityRecord> Identities { get; }
        IReadOnlyList<EvidenceRecord> Evidence { get; }
        IReadOnlyList<Match.MatchRecord> MatchIdentity { get; }
        IReadOnlyList<Match.MatchByIdentityKindIdRecord> MatchStudyIdentity { get; }
    }
}