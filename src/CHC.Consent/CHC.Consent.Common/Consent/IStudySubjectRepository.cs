using System;
using System.Collections.Generic;

namespace CHC.Consent.Common.Consent
{
    public interface IStudySubjectRepository
    {
        StudySubject GetStudySubject(StudyIdentity study, string subjectIdentifier);
        StudySubject FindStudySubject(StudyIdentity study, PersonIdentity personId);
        StudySubject AddStudySubject(StudySubject studySubject);
        StudySubject[] GetConsentedSubjects(StudyIdentity studyIdentity);
        IEnumerable<(StudySubject studySubject, DateTime? lastWithDrawn)> GetSubjectsWithLastWithdrawalDate(StudyIdentity studyIdentity);
    }
}