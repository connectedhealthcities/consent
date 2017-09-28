using System;
using System.Collections.Generic;

namespace CHC.Consent.Common.Core
{
    public interface IConsentStore
    {
        IConsent RecordConsent(Guid studyId, string subjectIdentifier, IEnumerable<IEvidence> evidence);
    }
}