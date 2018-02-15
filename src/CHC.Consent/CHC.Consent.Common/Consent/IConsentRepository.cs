using System.Collections.Generic;
using System.Linq;

namespace CHC.Consent.Common.Consent
{
    public interface IConsentRepository
    {
        Study GetStudy(string studyId);

        StudySubject FindStudySubject(Study studyId, string specificationSubjectIdentifier);
        StudySubject FindStudySubject(Study studyId, long personid);
        Consent FindActiveConsent(StudySubject studySubject, IEnumerable<Identifier> identifiers);

        void AddConsent(Consent consent);
        void AddStudySubject(StudySubject studySubject);
    }

    public static class ConsentRepositoryHelpers
    {
        public static Consent FindActiveConsent(
            this IConsentRepository repository, 
            StudySubject studySubject,
            params Identifier[] identifiers
            )
        {
            return repository.FindActiveConsent(studySubject, identifiers.AsEnumerable());
        }
    }
}