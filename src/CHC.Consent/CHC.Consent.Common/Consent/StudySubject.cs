using System;
using System.Collections.Generic;

namespace CHC.Consent.Common.Consent
{
    public class StudySubject
    {
        public string SubjectIdentifier { get; }
        public Study Study { get; }
        public long PersonId { get; }

        /// <inheritdoc />
        public StudySubject(Study study, string subjectIdentifier, long personId)
        {
            SubjectIdentifier = subjectIdentifier;
            Study = study;
            PersonId = personId;
        }

        public IEnumerable<Consent> Consents { get; } = Array.Empty<Consent>();
    }
}