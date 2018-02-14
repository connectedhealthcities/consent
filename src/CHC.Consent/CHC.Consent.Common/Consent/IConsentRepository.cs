namespace CHC.Consent.Common.Consent
{
    public interface IConsentRepository
    {
        Study GetStudy(string studyId);

        StudySubject FindStudySubject(Study studyId, string specificationSubjectIdentifier);
        StudySubject FindStudySubject(Study studyId, long personid);
        
        void AddConsent(Consent consent);
        void AddStudySubject(StudySubject studySubject);
    }
}