using System.Collections.Generic;
using CHC.Consent.Common.Import.Match;

namespace CHC.Consent.Common.Identity
{
    public interface IIdentityStore
    {
        IEnumerable<Identity> FindExisitingIdentiesFor(IReadOnlyCollection<Match> matches, IEnumerable<Identity> identities);
        void UpsertIdentity(IReadOnlyCollection<Match> match, IEnumerable<Identity> allIdentities);
    }
}