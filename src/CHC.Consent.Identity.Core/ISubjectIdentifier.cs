using System.Collections.Generic;

namespace CHC.Consent.Identity.Core
{
    public interface ISubjectIdentifier 
    {
        IEnumerable<IIdentity> Identities { get; }
    }
}