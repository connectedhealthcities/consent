using System.Collections.Generic;

namespace CHC.Consent.Identity.Core
{
    public interface IPerson
    {
        IEnumerable<IIdentity> Identities { get; }
        IEnumerable<ISubjectIdentifier> SubjectIdentifiers { get; }
    }
}