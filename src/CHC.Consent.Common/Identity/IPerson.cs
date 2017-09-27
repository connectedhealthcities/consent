using System.Collections.Generic;
using System.Security.Principal;
using CHC.Consent.Common.Core;

namespace CHC.Consent.Common.Identity
{
    public interface IPerson
    {
        IEnumerable<IIdentity> Identities { get; }
        IEnumerable<ISubjectIdentifier>SubjectIdentifiers { get; }
    }
}