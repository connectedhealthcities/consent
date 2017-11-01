using System;
using System.Collections.Generic;
using CHC.Consent.Core;

namespace CHC.Consent.Common.Core
{
    public interface IConsent
    {
        ISubject Subject { get; }
        
        DateTimeOffset DateProvisionRecorded { get; }
        IEnumerable<IEvidence> ProvidedEvidence { get; }
        
        DateTimeOffset? DateWithdrawlRecorded { get; }
        IEnumerable<IEvidence> WithdrawnEvidence { get; }
    }
}