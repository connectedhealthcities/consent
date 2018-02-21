using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.Common.Consent
{
    public class ConsentRepository : IConsentRepository
    {
        private IStore<Study> Studies { get; }
        private IStore<StudySubject> StudySubjects { get; }
        private IStore<Consent> Consents { get; }

        public ConsentRepository(
            IStore<Study> studies, IStore<StudySubject> studySubjects, IStore<Consent> consents)
        {
            Studies = studies;
            StudySubjects = studySubjects;
            Consents = consents;
        }

        public Study GetStudy(long studyId) => Studies.Get(studyId);

        public StudySubject FindStudySubject(Study study, string subjectIdentifier) =>
            SubjectsForStudy(study).SingleOrDefault(_ =>  _.SubjectIdentifier == subjectIdentifier);

        private IQueryable<StudySubject> SubjectsForStudy(Study study) =>
            StudySubjects.Where(_ => Equals(_.Study, study));

        public StudySubject FindStudySubject(Study study, long personId) => 
            SubjectsForStudy(study).SingleOrDefault(_ => _.PersonId == personId);

        public Consent FindActiveConsent(StudySubject studySubject, IEnumerable<Identifier> identifiers) =>
            ConsentsForStudySubject(studySubject).SingleOrDefault(_ => _.Withdrawn == null);

        private IQueryable<Consent> ConsentsForStudySubject(StudySubject studySubject) =>
            Consents.Where(_ => Equals(_.StudySubject, studySubject));

        public Consent AddConsent(Consent consent) => 
            Consents.Add(consent);

        public StudySubject AddStudySubject(StudySubject studySubject) => 
            StudySubjects.Add(studySubject);
    }
}