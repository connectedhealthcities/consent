using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Security;
using NeinLinq;


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

        public ConsentIdentity FindActiveConsent(StudySubject studySubject, IEnumerable<CaseIdentifier> caseIdentifiers)
        {
            var consents =
                Consents.Where(
                    _ => _.StudySubject.Person.Id == studySubject.PersonId.Id &&
                         _.StudySubject.Study.Id == studySubject.StudyId.Id &&
                         _.StudySubject.SubjectIdentifier == studySubject.SubjectIdentifier &&
                         _.DateWithdrawn == null);
                

            if (!caseIdentifiers.Any())
            {
                consents = consents.Where(consent => CaseIdentifiers.All(caseId => caseId.Consent != consent));
            }
            else
            {
                foreach (var caseIdentifier in caseIdentifiers)
                {
                    var xmlMarshaller = GetMarshaller(caseIdentifier);
                    var value = xmlMarshaller.MarshalledValue(caseIdentifier);
                    var type = xmlMarshaller.ValueType;
                    consents = consents.Where(
                        _ => CaseIdentifiers.Any(id => id.Consent == _ && id.Type == type && id.Value == value));
                }
            }

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

            foreach (var caseIdentifier in consent.CaseIdentifiers)
            {
                var xmlMarshaller = GetMarshaller(caseIdentifier);
                CaseIdentifiers.Add(
                    new CaseIdentifierEntity
                    {
                        Value = xmlMarshaller.MarshalledValue(caseIdentifier),
                        Type = xmlMarshaller.ValueType,
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


        public IEnumerable<Study> GetStudies(IUserProvider user)
        {
            return Studies.ToInjectable()
                .WithReadPermissionGrantedTo(user)
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
            var consentDetails = (
                    from c in Consents.ToInjectable()
                        .WithReadPermissionGrantedTo(user)
                    let subject = c.StudySubject
                    where c.DateWithdrawn == null
                          && subject.SubjectIdentifier == subjectIdentifier
                          && subject.Study.Id == studyId.Id
                    select new
                    {
                        c.Id,
                        c.DateProvided,
                        PersonId = subject.Person.Id,
                        GivenBy = c.GivenBy.Id,
                    }
                )
                .ToArray();
            var consentIds = consentDetails.Select(c => c.Id).ToArray();
            var evidences =
                Evidence
                    .OfType<GivenEvidenceEntity>()
                    .Where(e => consentIds.Contains(e.Consent.Id))
                    .GroupBy(e => e.Consent.Id)
                    .ToDictionary(e => e.Key, e => e.Select(Unmarshall));
            
            var caseIdentifiers =
                CaseIdentifiers
                    .Where(c => consentIds.Contains(c.Consent.Id))
                    .GroupBy(c => c.Consent.Id)
                    .ToDictionary(c => c.Key, c => c.Select(Unmarshall));

            return consentDetails
                .Select(
                    _ => new Common.Consent.Consent(
                        new StudySubject(studyId, subjectIdentifier, new PersonIdentity(_.PersonId)),
                        _.DateProvided,
                        _.GivenBy,
                        evidences.TryGetValue(_.Id, out var evidence) ? evidence : Array.Empty<Evidence>(),
                        caseIdentifiers.TryGetValue(_.Id, out var caseIds)
                            ? caseIds
                            : Array.Empty<CaseIdentifier>()));
        }


        private Evidence Unmarshall(EvidenceEntity evidenceEntity) =>
            (Evidence) GetXmlMarshaller(evidenceEntity)
                .Unmarshall(evidenceEntity.Type, evidenceEntity.Value);
        
        private CaseIdentifier Unmarshall(CaseIdentifierEntity caseIdentifierEntity) => 
            (CaseIdentifier)GetXmlMarshaller(caseIdentifierEntity)
            .Unmarshall(caseIdentifierEntity.Type, caseIdentifierEntity.Value);
        
        private XmlMarshaller GetXmlMarshaller(EvidenceEntity evidenceEntity)
        {
            var caseIdentifierType = EvidenceRegistry[evidenceEntity.Type];
            var xmlMarshaller = new XmlMarshaller(caseIdentifierType, evidenceEntity.Type);
            return xmlMarshaller;
        }

        private XmlMarshaller GetXmlMarshaller(CaseIdentifierEntity caseIdentifierEntity)
        {
            var caseIdentifierType = CaseIdentifierRegistry[caseIdentifierEntity.Type];
            var xmlMarshaller = new XmlMarshaller(caseIdentifierType, caseIdentifierEntity.Type);
            return xmlMarshaller;
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

        private XmlMarshaller GetMarshaller(CaseIdentifier caseIdentifier)
        {
            var typeName = CaseIdentifierRegistry[caseIdentifier.GetType()];
            var xmlMarshaller = new XmlMarshaller(caseIdentifier.GetType(), typeName);
            return xmlMarshaller;
        }
    }
}