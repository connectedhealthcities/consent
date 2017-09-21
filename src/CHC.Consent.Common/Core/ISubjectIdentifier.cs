using System;

namespace CHC.Consent.Common.Core
{
    public interface ISubjectIdentifier
    {
        Guid StudyId { get; }
        string Identifier { get; }
    }
}