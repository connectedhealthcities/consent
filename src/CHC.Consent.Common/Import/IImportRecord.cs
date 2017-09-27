using System.Collections.Generic;
using CHC.Consent.Common.Evidence;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.Common.Import
{
    public interface IImportRecord
    {
        IReadOnlyList<IdentityRecord> Identities { get; }
        IReadOnlyList<Evidence.Evidence> Evidence { get; }
        IReadOnlyList<Match.Match> MatchIdentity { get; }
        IReadOnlyList<Match.Match> MatchStudyIdentity { get; }
    }
}