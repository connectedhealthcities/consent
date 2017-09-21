using System.Collections.Generic;

namespace CHC.Consent.Common.Identity
{
    public interface IIdentityStore
    {
        IEnumerable<Identity> FindExisitingIdentiesFor(IEnumerable<Identity> keys);
        void UpsertIdentity(IEnumerable<Identity> keys, IEnumerable<Identity> allIdentities);
    }
}