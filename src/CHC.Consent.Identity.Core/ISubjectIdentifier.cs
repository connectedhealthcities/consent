using System;
using System.Collections.Generic;

namespace CHC.Consent.Identity.Core
{
    public interface ISubjectIdentifier 
    {
        Guid StudyId { get; }
        string SubjectIdentifier { get; }
        IEnumerable<IIdentity> Identities { get; }
    }
}