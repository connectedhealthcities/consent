using System.Collections.Generic;
using CHC.Consent.Import.Core;

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