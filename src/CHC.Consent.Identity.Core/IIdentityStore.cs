using System.Collections.Generic;

namespace CHC.Consent.Identity.Core
{
    /// <remarks>
    /// TODO: Does this need to operate on a higher level than Match?
    /// TODO: Use IPerson rather than collections of IIdentity
    /// </remarks>
    public interface IIdentityStore
    {
        IPerson FindPerson(IReadOnlyCollection<IMatch> matches);
        IPerson CreatePerson(IEnumerable<IIdentity> identities);
    }
}