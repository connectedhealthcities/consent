using System.Collections.Generic;

namespace CHC.Consent.Common.Consent
{
    public interface IConsentRepository
    {
        StudyIdentity GetStudy(long studyId);

        StudySubject FindStudySubject(StudyIdentity study, string subjectIdentifier);
        StudySubject FindStudySubject(StudyIdentity study, PersonIdentity personId);
        StudySubject AddStudySubject(StudySubject studySubject);
        
        ConsentIdentity FindActiveConsent(StudySubject studySubject, IEnumerable<CaseIdentifier> caseIdentifiers);
        ConsentIdentity AddConsent(Consent consent);
    }
}