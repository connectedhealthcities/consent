using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    public class StudySubjectRepository : IStudySubjectRepository
    {
        private StudyRepository Studies { get; }
        private IStore<StudySubjectEntity> StudySubjects { get; }
        private IStore<PersonEntity> People { get; }
        private IStore<ConsentEntity> Consents { get; }

        public StudySubjectRepository(StudyRepository studies, IStore<StudySubjectEntity> studySubjects, IStore<PersonEntity> people, IStore<ConsentEntity> consents)
        {
            Studies = studies;
            StudySubjects = studySubjects;
            People = people;
            Consents = consents;
        }

        StudySubject IStudySubjectRepository.GetStudySubject(StudyIdentity study, string subjectIdentifier) =>
            GetStudySubject(SubjectsForStudy(study).Where(_ =>  _.SubjectIdentifier == subjectIdentifier));

        StudySubject IStudySubjectRepository.FindStudySubject(StudyIdentity study, PersonIdentity personId)
        {
            return GetStudySubject(FindStudySubjectForPerson(study, personId));
        }

        /// <inheritdoc />
        IEnumerable<(StudySubject studySubject, DateTime? lastWithDrawn)> IStudySubjectRepository.GetSubjectsWithLastWithdrawalDate(StudyIdentity studyIdentity)
        {
            return
                (from subject in StudySubjects
                    where subject.Study.Id == studyIdentity.Id &&
                          Consents.Any(_ => _.StudySubject == subject)
                    select new
                    {
                        subject.SubjectIdentifier,
                        PersonId = subject.Person.Id,
                        MostRecentWithdrawal = Consents.Where(c => c.StudySubject == subject)
                            .OrderByDescending(c => c.DateProvided)
                            .Select(_ => _.DateWithdrawn).FirstOrDefault()
                    }
                ).AsEnumerable()
                .Select(
                    _ => (new StudySubject(studyIdentity, _.SubjectIdentifier, new PersonIdentity(_.PersonId)),
                        _.MostRecentWithdrawal))
                .ToArray();
        }

        StudySubject IStudySubjectRepository.AddStudySubject(StudySubject studySubject)
        {
            var study = Studies.GetStudy(studySubject.StudyId.Id);
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
                new StudyIdentity((long) saved.Study.Id),
                saved.SubjectIdentifier,
                new PersonIdentity((long) saved.Person.Id));
        }

        public IQueryable<StudySubjectEntity> SubjectsForStudy(StudyIdentity study) =>
            StudySubjects.Where(_ => _.Study.Id == study.Id);

        public IQueryable<StudySubjectEntity> FindStudySubjectForPerson(StudyIdentity study, PersonIdentity personId)
        {
            return SubjectsForStudy(study).Where(_ => _.Person.Id == personId.Id);
        }

        private StudySubject GetStudySubject(IQueryable<StudySubjectEntity> entity)
        {
            //explicit conversions required as EF thinks Study and Person might be null, and hence Ids could be nullable
            // ReSharper disable RedundantCast
            return entity
                .Select(
                    _ => new StudySubject(
                        new StudyIdentity((long)_.Study.Id),
                        _.SubjectIdentifier,
                        new PersonIdentity((long)_.Person.Id)))
                .SingleOrDefault();
            // ReSharper restore RedundantCast
        }

        public StudySubject[] GetConsentedSubjects(StudyIdentity studyIdentity)
        {
            return StudySubjects.Where(
                    s => s.Study.Id == studyIdentity.Id &&
                         Consents.Any(c => c.StudySubject.Id == s.Id && c.DateWithdrawn == null))
                .Select(_ => new StudySubject(studyIdentity, _.SubjectIdentifier, new PersonIdentity(_.Person.Id)))
                .ToArray();
        }
    }
}