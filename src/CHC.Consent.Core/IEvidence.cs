using System;

namespace CHC.Consent.Common.Core
{
    public interface IEvidence
    {
        Guid EvidenceKindId { get; }
        string Evidence { get; }
    }
}