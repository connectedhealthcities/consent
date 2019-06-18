using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using System.Xml.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Security;
using Microsoft.EntityFrameworkCore;
using NeinLinq;


namespace CHC.Consent.EFCore
{
    
    public class ConsentRepository : IConsentRepository
    {
        private IStore<StudyEntity> Studies { get; }
        private IStore<StudySubjectEntity> StudySubjects { get; }
        private IStore<PersonEntity> People { get; }
        private IStore<ConsentEntity> Consents { get; }
        private IStore<EvidenceEntity> Evidence { get; }
        private IdentifierXmlMarshallers<Evidence, EvidenceDefinition> EvidenceRegistry { get; }

        public ConsentRepository(
            IStore<StudyEntity> studies, 
            IStore<StudySubjectEntity> studySubjects,
            IStore<PersonEntity> people, 
            IStore<ConsentEntity> consents,
            IStore<EvidenceEntity> evidence,
            EvidenceDefinitionRegistry evidenceRegistry)
        {
            Studies = studies;
            StudySubjects = studySubjects;
            People = people;
            Consents = consents;
            Evidence = evidence;
            EvidenceRegistry = new IdentifierXmlMarshallers<Evidence, EvidenceDefinition>(evidenceRegistry);
            
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

        public StudySubject FindStudySubject(StudyIdentity study, PersonIdentity personId)
        {
            return GetStudySubject(FindStudySubjectForPerson(study, personId));
        }

        private IQueryable<StudySubjectEntity> FindStudySubjectForPerson(StudyIdentity study, PersonIdentity personId)
        {
            return SubjectsForStudy(study).Where(_ => _.Person.Id == personId.Id);
        }

        public ConsentIdentity FindActiveConsent(StudySubject studySubject)
        {
            var consents =
                Consents.Where(
                    _ => _.StudySubject.Person.Id == studySubject.PersonId.Id &&
                         _.StudySubject.Study.Id == studySubject.StudyId.Id &&
                         _.StudySubject.SubjectIdentifier == studySubject.SubjectIdentifier &&
                         _.DateWithdrawn == null);
                

            var consentId = consents.Select(_ => (long?)_.Id).SingleOrDefault();
            return consentId == null ? null : new ConsentIdentity(consentId.Value);
        }

        /// <inheritdoc />
        public ConsentIdentity AddConsent(Common.Consent.Consent consent)
        {
            var subject = FindStudySubjectForPerson(consent.StudySubject.StudyId, consent.StudySubject.PersonId).FirstOrDefault();
            
            if(subject == null) throw new NotImplementedException($"StudySubject#{consent.StudySubject} not found");

            var givenBy = People.Get(consent.GivenByPersonId);
            if(givenBy == null) throw new NotImplementedException($"Person#{consent.GivenByPersonId} not found");

            var saved = Consents.Add(
                new ConsentEntity {DateProvided = consent.DateGiven, GivenBy = givenBy, StudySubject = subject});

            foreach (var evidence in consent.GivenEvidence)
            {
                Evidence.Add(
                    new GivenEvidenceEntity
                    {
                        Value = EvidenceRegistry.MarshallToXml(evidence).ToString(SaveOptions.DisableFormatting),
                        Type = evidence.Definition.SystemName,
                        Consent = saved
                    });
            }

            return new ConsentIdentity(saved.Id);
        }

        public IEnumerable<Study> GetStudies(IUserProvider user)
        {
            return Studies.ToInjectable()
                //.WithReadPermissionGrantedTo(user)
                .Select(_ => new Study(_.Id, _.Name))
                .ToArray();
        }

        /// <inheritdoc />
        public StudySubject[] GetConsentedSubjects(StudyIdentity studyIdentity)
        {
            return StudySubjects.Where(
                    s => s.Study.Id == studyIdentity.Id &&
                         Consents.Any(c => c.StudySubject.Id == s.Id && c.DateWithdrawn == null))
                .Select(_ => new StudySubject(studyIdentity, _.SubjectIdentifier, new PersonIdentity(_.Person.Id)))
                .ToArray();
        }

        /// <inheritdoc />
        public IEnumerable<Common.Consent.Consent> GetActiveConsentsForSubject(
            StudyIdentity studyId,
            string subjectIdentifier,
            IUserProvider user)
        {
            var consentDetails = 
                (
                    from c in ConsentsFor(studyId.Id, subjectIdentifier).Include(_ => _.GivenEvidence).ToInjectable()
                       // .WithReadPermissionGrantedTo(user)
                    let subject = c.StudySubject
                    where c.DateWithdrawn == null
                    select new
                    {
                        c.Id,
                        c.DateProvided,
                        PersonId = subject.Person.Id,
                        GivenBy = c.GivenBy.Id,
                        c.GivenEvidence
                    }
                )
                .ToArray();
            
            return consentDetails
                .Select(
                    _ => new Common.Consent.Consent(
                        new StudySubject(studyId, subjectIdentifier, new PersonIdentity(_.PersonId)),
                        _.DateProvided,
                        _.GivenBy,
                        _.GivenEvidence.AsQueryable().Select(MarshallEvidenceFromEntity())
                        )
                );
        }

        private Expression<Func<EvidenceEntity, Evidence>> MarshallEvidenceFromEntity()
        {
            return evidence => EvidenceRegistry.MarshallFromXml(evidence.Type, XElement.Parse(evidence.Value));
        }
        

        /// <inheritdoc />
        public IEnumerable<(StudySubject studySubject, DateTime? lastWithDrawn)> GetSubjectsWithLastWithdrawalDate(StudyIdentity studyIdentity)
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

        /// <inheritdoc />
        public void WithdrawConsent(StudySubject studySubject, params Evidence[] allEvidence)
        {
            var activeConsent = ConsentsFor(studySubject.StudyId.Id, studySubject.SubjectIdentifier)
                .Where(_ => _.DateWithdrawn == null)
                .OrderByDescending(_ => _.DateProvided)
                .FirstOrDefault();
            
            activeConsent.DateWithdrawn = DateTime.Now;
            foreach (var evidence in allEvidence)
            {
                activeConsent.WithdrawnEvidence.Add(
                    new WithdrawnEvidenceEntity
                    {
                        Type = evidence.Definition.SystemName,
                        Value = EvidenceRegistry.MarshallToXml(evidence).ToString(SaveOptions.DisableFormatting),

                    }
                );
            }
        }

        private IQueryable<ConsentEntity> ConsentsFor(long studyId, string subjectIdentifier)
        {
            return Consents.Where(
                c => c.StudySubject.SubjectIdentifier == subjectIdentifier &&
                     c.StudySubject.Study.Id == studyId);
        }

        /// <inheritdoc />
        public IEnumerable<Common.Consent.Consent> GetConsentsForSubject(StudyIdentity studyIdentity, string subjectIdentifier, IUserProvider user)
        {
            return
                from c in (
                from c in ConsentsFor(studyIdentity, subjectIdentifier)
                    .Include(_ => _.GivenEvidence).Include(_ => _.WithdrawnEvidence)
                orderby c.DateProvided descending
                let subject = c.StudySubject
                let person = subject.Person
                select new
                {
                    StudySubject = new StudySubject(studyIdentity, subjectIdentifier, 
                        new PersonIdentity(person.Id)), 
                    c.DateProvided, 
                    c.DateWithdrawn,
                    GivenById = c.GivenBy.Id,
                    GivenEvidence = c.GivenEvidence.ToArray(),
                    WithdrawnEvidence = c.WithdrawnEvidence.ToArray()
                }
                ).ToArray()
                select new Common.Consent.Consent(
                    c.StudySubject,
                    c.DateProvided,
                    c.GivenById,
                    c.GivenEvidence.AsQueryable().Select(MarshallEvidenceFromEntity()))
                {
                    DateWithdrawn = c.DateWithdrawn,
                    WithdrawnEvidence = c.WithdrawnEvidence.AsQueryable().Select(MarshallEvidenceFromEntity())
                };

        }

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
                new StudyIdentity((long) saved.Study.Id),
                saved.SubjectIdentifier,
                new PersonIdentity((long) saved.Person.Id));
        }
    }
}