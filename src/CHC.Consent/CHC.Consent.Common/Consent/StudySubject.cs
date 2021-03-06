﻿using System;
using System.Collections.Generic;

namespace CHC.Consent.Common.Consent
{
    public class StudySubject
    {
        public string SubjectIdentifier { get; protected set; }
        public StudyIdentity StudyId { get; protected set; }
        public PersonIdentity PersonId { get; protected set; }

        /// <inheritdoc />
        public StudySubject(StudyIdentity studyId, string subjectIdentifier, PersonIdentity personId)
        {
            SubjectIdentifier = subjectIdentifier;
            StudyId = studyId ?? throw new ArgumentNullException(nameof(studyId));
            PersonId = personId ?? throw new ArgumentNullException(nameof(personId));
        }
    }
}