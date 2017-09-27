using System.Collections.Generic;
using CHC.Consent.Common.Import.Match;

namespace CHC.Consent.Common.Identity
{
    public interface IIdentityStore
    {
        IEnumerable<IIdentity> FindExisitingIdentiesFor(IReadOnlyCollection<Match> matches, IEnumerable<IIdentity> identities);
        void UpsertIdentity(IReadOnlyCollection<Match> matches, IEnumerable<IIdentity> allIdentities);
        
    }
}