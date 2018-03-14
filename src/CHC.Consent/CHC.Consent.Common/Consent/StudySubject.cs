using System;
using System.Collections.Generic;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.Common.Consent
{
    public class StudySubject
    {
        public long Id { get; protected set; }

        public string SubjectIdentifier { get; protected set; }
        public StudyIdentity StudyId { get; protected set; }
        public PersonIdentity PersonId { get; protected set; }


        /// <inheritdoc />
        public StudySubject(long id, StudyIdentity studyId, string subjectIdentifier, PersonIdentity personId)
            : this(studyId, subjectIdentifier, personId)
        {
            Id = id;
            SubjectIdentifier = subjectIdentifier;
            StudyId = studyId;
            PersonId = personId;
        }

        /// <inheritdoc />
        public StudySubject(StudyIdentity studyId, string subjectIdentifier, PersonIdentity personId)
        {
            SubjectIdentifier = subjectIdentifier;
            StudyId = studyId ?? throw new ArgumentNullException(nameof(studyId));
            PersonId = personId ?? throw new ArgumentNullException(nameof(personId));
        }
    }
}