using System;
using System.Collections.Generic;

namespace CHC.Consent.Common.Core
{
    public interface IConsent
    {
        Guid StudyId { get; }
        string SubjectIdentifier { get; }
        
        DateTimeOffset DateProvisionRecorded { get; }
        IEnumerable<IEvidence> ProvidedEvidence { get; }
        
        DateTimeOffset? DateWithdrawlRecorded { get; }
        IEnumerable<IEvidence> WithdrawnEvidence { get; }
    }
}