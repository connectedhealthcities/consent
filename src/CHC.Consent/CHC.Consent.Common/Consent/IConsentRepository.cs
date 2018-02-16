using System.Collections.Generic;
using System.Linq;

namespace CHC.Consent.Common.Consent
{
    public interface IConsentRepository
    {
        Study GetStudy(string studyId);

        StudySubject FindStudySubject(Study study, string subjectIdentifier);
        StudySubject FindStudySubject(Study study, long personId);
        Consent FindActiveConsent(StudySubject studySubject, IEnumerable<Identifier> identifiers);

        Consent AddConsent(Consent consent);
        StudySubject AddStudySubject(StudySubject studySubject);
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