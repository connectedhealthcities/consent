using System;
using System.Collections.Generic;
using CHC.Consent.Common.Core;
using CHC.Consent.Security;

namespace CHC.Consent.Identity.Core
{
    public interface IPerson : ISecurable
    {
        Guid Id { get; }
        IEnumerable<IIdentity> Identities { get; }
        IEnumerable<ISubjectIdentifier> SubjectIdentifiers { get; }
        ISubjectIdentifier AddSubjectIdentifier(IStudy study, string subjectIdentifer, IEnumerable<IIdentity> identities);
    }
}