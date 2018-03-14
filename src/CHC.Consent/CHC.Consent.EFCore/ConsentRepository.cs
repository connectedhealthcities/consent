using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    public class ConsentRepository : IConsentRepository
    {
        private IStore<StudyEntity> Studies { get; }
        private IStore<StudySubjectEntity> StudySubjects { get; }
        private IStore<PersonEntity> People { get; }
        private IStore<Common.Consent.Consent> Consents { get; }

        public ConsentRepository(
            IStore<StudyEntity> studies, 
            IStore<StudySubjectEntity> studySubjects,
            IStore<PersonEntity> People, 
            IStore<Common.Consent.Consent> consents)
        {
            Studies = studies;
            StudySubjects = studySubjects;
            this.People = People;
            Consents = consents;
        }

        public StudyIdentity GetStudy(long studyId) =>
            Studies.Where(_ => _.Id == studyId)
                .Select(_ => new StudyIdentity(studyId)).SingleOrDefault();

        private IQueryable<StudySubjectEntity> SubjectsForStudy(StudyIdentity study) =>
            StudySubjects.Where(_ => _.Study.Id == study.Id);

        public StudySubject FindStudySubject(StudyIdentity study, string subjectIdentifier) =>
            GetStudySubject(SubjectsForStudy(study).Where(_ =>  _.SubjectIdentifier == subjectIdentifier));

        private StudySubject GetStudySubject(IQueryable<StudySubjectEntity> entity)
        {
            //explicit conversions required as EF thinks Study and Person might be null 
            return entity
                .Select(
                    _ => new StudySubject(
                        _.Id,
                        new StudyIdentity((long)_.Study.Id),
                        _.SubjectIdentifier,
                        new PersonIdentity((long)_.Person.Id)))
                .SingleOrDefault();
        }

        public StudySubject FindStudySubject(StudyIdentity study, PersonIdentity personId)
        {
            return GetStudySubject(SubjectsForStudy(study).Where(_ => _.Person.Id == personId.Id));
        }

        public Common.Consent.Consent FindActiveConsent(StudySubject studySubject, IEnumerable<ConsentIdentifier> identifiers) =>
            throw new NotImplementedException();

        /// <inheritdoc />
        public Common.Consent.Consent AddConsent(StudySubject subject, Common.Consent.Consent consent) =>
            throw new NotImplementedException();

        public StudySubject AddStudySubject(StudySubject studySubject)
        {
            var study = Studies.Get(studySubject.StudyId.Id);
            if(study == null) throw new NotImplementedException($"{studySubject.StudyId} not found");

            var person = People.Get(studySubject.PersonId.Id);
            if(person == null) throw new NotImplementedException($"{studySubject.PersonId} not found");

            var saved = StudySubjects.Add(
                new StudySubjectEntity
                {
                    Person = person,
                    Study = study,
                    SubjectIdentifier = studySubject.SubjectIdentifier
                });

            return new StudySubject(
                saved.Id,
                new StudyIdentity((long) saved.Study.Id),
                saved.SubjectIdentifier,
                new PersonIdentity((long) saved.Person.Id));
        }
    }
}