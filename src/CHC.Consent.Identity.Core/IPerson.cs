using System.Collections.Generic;
using CHC.Consent.Common.Core;

namespace CHC.Consent.Identity.Core
{
    public interface IPerson
    {
        IEnumerable<IIdentity> Identities { get; }
        IEnumerable<ISubjectIdentifier> SubjectIdentifiers { get; }
        ISubjectIdentifier AddSubjectIdentifier(IStudy study, string subjectIdentifer, IEnumerable<IIdentity> identities);
    }
}