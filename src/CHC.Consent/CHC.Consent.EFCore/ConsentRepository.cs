using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure;
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
        private IStore<ConsentEntity> Consents { get; }
        private IStore<CaseIdentifierEntity> CaseIdentifiers { get; }
        private IStore<EvidenceEntity> Evidence { get; }
        private ITypeRegistry<CaseIdentifier> CaseIdentifierRegistry { get; }
        private ITypeRegistry<Evidence> EvidenceRegistry { get; }

        public ConsentRepository(
            IStore<StudyEntity> studies, 
            IStore<StudySubjectEntity> studySubjects,
            IStore<PersonEntity> people, 
            IStore<ConsentEntity> consents,
            IStore<CaseIdentifierEntity> caseIdentifiers,
            IStore<EvidenceEntity> evidence,
            ITypeRegistry<CaseIdentifier> caseIdentifierRegistry,
            ITypeRegistry<Evidence> evidenceRegistry)
        {
            Studies = studies;
            StudySubjects = studySubjects;
            People = people;
            Consents = consents;
            CaseIdentifiers = caseIdentifiers;
            Evidence = evidence;
            CaseIdentifierRegistry = caseIdentifierRegistry;
            EvidenceRegistry = evidenceRegistry;
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
                        _.Id,
                        new StudyIdentity((long)_.Study.Id),
                        _.SubjectIdentifier,
                        new PersonIdentity((long)_.Person.Id)))
                .SingleOrDefault();
            // ReSharper restore RedundantCast
        }

        public StudySubject FindStudySubject(StudyIdentity study, PersonIdentity personId)
        {
            return GetStudySubject(SubjectsForStudy(study).Where(_ => _.Person.Id == personId.Id));
        }

        public ConsentIdentity FindActiveConsent(StudySubject studySubject, IEnumerable<CaseIdentifier> identifiers) =>
            throw new NotImplementedException();

        /// <inheritdoc />
        public ConsentIdentity AddConsent(Common.Consent.Consent consent)
        {
            var subject = StudySubjects.Get(consent.StudySubject.Id);
            if(subject == null) throw new NotImplementedException($"StudySubject#{consent.StudySubject.Id} not found");

            var givenBy = People.Get(consent.GivenByPersonId);
            if(givenBy == null) throw new NotImplementedException($"Person#{consent.GivenByPersonId} not found");

            var saved = Consents.Add(
                new ConsentEntity {DateProvided = consent.DateGiven, GivenBy = givenBy, StudySubject = subject});

            foreach (var caseIdentifier in consent.CaseIdentifiers)
            {
                var typeName = CaseIdentifierRegistry[caseIdentifier.GetType()];

                CaseIdentifiers.Add(
                    new CaseIdentifierEntity
                    {
                        Value = new XmlMarshaller(caseIdentifier.GetType(), typeName)
                            .MarshalledValue(caseIdentifier),
                        Type = typeName,
                        Consent = saved
                    });
            }

            foreach (var evidence in consent.GivenEvidence)
            {
                var typeName = EvidenceRegistry[evidence.GetType()];

                Evidence.Add(
                    new GivenEvidenceEntity
                    {
                        Value = new XmlMarshaller(evidence.GetType(), typeName)
                            .MarshalledValue(evidence),
                        Type = typeName,
                        Consent = saved
                    });
            }

            return new ConsentIdentity(saved.Id);
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
                saved.Id,
                new StudyIdentity((long) saved.Study.Id),
                saved.SubjectIdentifier,
                new PersonIdentity((long) saved.Person.Id));
        }
    }
}