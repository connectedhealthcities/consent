using System;
using System.Collections.Generic;
using CHC.Consent.Common.Infrastructure;

namespace CHC.Consent.Common.Consent
{
    public interface IConsentRepository
    {
        StudyIdentity GetStudy(long studyId);

        StudySubject FindStudySubject(StudyIdentity study, string subjectIdentifier);
        StudySubject FindStudySubject(StudyIdentity study, PersonIdentity personId);
        StudySubject AddStudySubject(StudySubject studySubject);

        ConsentIdentity FindActiveConsent(StudySubject studySubject);
        ConsentIdentity AddConsent(Consent consent);

        IEnumerable<Study> GetStudies(IUserProvider user);
        StudySubject[] GetConsentedSubjects(StudyIdentity studyIdentity);
        IEnumerable<Consent> GetActiveConsentsForSubject(
            StudyIdentity studyId, string subjectIdentifier, IUserProvider user);

        IEnumerable<(StudySubject studySubject, DateTime? lastWithDrawn)> GetSubjectsWithLastWithdrawalDate(StudyIdentity studyIdentity);
        void WithdrawConsent(StudySubject studySubject, params Evidence[] allEvidence);
        IEnumerable<Consent> GetConsentsForSubject(StudyIdentity studyIdentity, string subjectIdentifier, IUserProvider user);
    }
}