using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Consent.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.Testing.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Random = CHC.Consent.Testing.Utils.Random;

namespace CHC.Consent.EFCore.Tests
{
    using Consent = Common.Consent.Consent;
    [Collection(DatabaseCollection.Name)]
    public class ConsentRepositoryTests : DbTests
    {
        private readonly StudyEntity study;
        private readonly ConsentRepository repository;
        private readonly StudySubjectEntity consentedStudySubject;
        
        private readonly ConsentEntity activeConsent;
        
        private ConsentContext readContext;
        private ConsentContext updateContext;
        private ConsentContext createContext;
        private readonly StudyIdentity studyId;
        private readonly PersonIdentity consentedPersonId;
        private readonly PersonIdentity unconsentedPersonId;

        /// <inheritdoc />
        public ConsentRepositoryTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            readContext = CreateNewContextInSameTransaction();
            updateContext = CreateNewContextInSameTransaction();
            createContext = CreateNewContextInSameTransaction();


            study = createContext.Studies.Add(new StudyEntity{Name = Random.String()}).Entity;
            
            var consentedPerson = createContext.People.Add(new PersonEntity()).Entity;
            consentedStudySubject = createContext.Add(new StudySubjectEntity{ Study = study, Person = consentedPerson, SubjectIdentifier = "Consented"}).Entity;
            createContext.Add(
                new ConsentEntity
                {
                    StudySubject = consentedStudySubject,
                    DateProvided = 1.December(1965),
                    DateWithdrawn = 1.January(1966),
                    GivenBy = consentedPerson
                });
            
            activeConsent = createContext.Add(
                new ConsentEntity
                {
                    StudySubject = consentedStudySubject,
                    DateProvided = 1.February(1966),
                    GivenBy = consentedPerson
                }).Entity;
            
            var unconsentedPerson = createContext.People.Add(new PersonEntity()).Entity;
            createContext.Add(
                new StudySubjectEntity {Study = study, Person = unconsentedPerson, SubjectIdentifier = "Unconsented"});
            
            createContext.SaveChanges();
            
            studyId = new StudyIdentity(study.Id);
            consentedPersonId = consentedPerson;
            unconsentedPersonId = unconsentedPerson;
            
            repository = CreateRepository(readContext);

            
            
           
        }

        private ConsentRepository CreateRepository(
            ConsentContext consentContext, 
            Type[] caseIdentifierTypes=null, 
            Type[] evidentTypes=null)
        {
            var storeProvider = (IStoreProvider) new ContextStoreProvider(consentContext);
            var caseIdentifierRegistry = new TypeRegistry<CaseIdentifier, CaseIdentifierAttribute>();
            foreach (var caseIdentifierType in caseIdentifierTypes??new[] { typeof(PregnancyNumberIdentifier) })
            {
                 caseIdentifierRegistry.Add(caseIdentifierType);   
            }
            var evidenceRegsitry = new TypeRegistry<Evidence, EvidenceAttribute>();
            foreach (var evidenceType in evidentTypes??new [] { typeof(MedwayEvidence) })
            {
                evidenceRegsitry.Add(evidenceType);
            }
            
            return new ConsentRepository(
                storeProvider.Get<StudyEntity>(),
                storeProvider.Get<StudySubjectEntity>(),
                storeProvider.Get<PersonEntity>(),
                storeProvider.Get<ConsentEntity>(),
                storeProvider.Get<CaseIdentifierEntity>(),
                storeProvider.Get<EvidenceEntity>(),
                caseIdentifierRegistry,
                evidenceRegsitry
                );
        }

        [Fact]
        public void CanAddAStudySubject()
        {
            var person = createContext.Add(new PersonEntity()).Entity;
            createContext.SaveChanges();
            var personId = new PersonIdentity(person.Id);

            var subjectIdentifier = Random.String();
            var addedSubject = CreateRepository(updateContext).AddStudySubject(
                new StudySubject(studyId, subjectIdentifier, personId));
            updateContext.SaveChanges();
            
            
            Assert.NotNull(addedSubject);
            Assert.Equal(studyId, addedSubject.StudyId);
            Assert.Equal(personId, addedSubject.PersonId);
            Assert.Equal(subjectIdentifier, addedSubject.SubjectIdentifier);

            var studySubjectEntity = readContext.Set<StudySubjectEntity>()
                .Include(_ => _.Study)
                .Include(_ => _.Person)
                .SingleOrDefault(
                _ => _.Study.Id == addedSubject.StudyId && _.Person.Id == addedSubject.PersonId);
            Assert.Equal(subjectIdentifier, studySubjectEntity.SubjectIdentifier);
            Assert.Equal(studyId.Id, studySubjectEntity.Study.Id);
            Assert.Equal(personId.Id, studySubjectEntity.Person.Id);
        }

        [Fact]
        public void StoresConsentHeaderWhenAddingConsent()
        {
            var subjectIdentifier = Random.String();
            var person = createContext.Add(new PersonEntity()).Entity;
            var studySubjectEntity = createContext.Add(
                new StudySubjectEntity
                {
                    Person = person,
                    Study = createContext.Find<StudyEntity>(study.Id),
                    SubjectIdentifier = subjectIdentifier
                }).Entity;
            createContext.SaveChanges();
            
            var personId = new PersonIdentity(person.Id);
            var studySubject = new StudySubject(studyId, subjectIdentifier, personId);

            var dateGiven = Random.Date().Date;


            var consent = CreateRepository(updateContext)
                .AddConsent(
                    new Consent(studySubject, dateGiven, personId, null, Enumerable.Empty<CaseIdentifier>())
                );
            
            updateContext.SaveChanges();
            

            var consentEntity = readContext.Set<ConsentEntity>()
                .Include(_ => _.GivenBy)
                .Include(_ => _.StudySubject)
                    .ThenInclude(_ => _.Study)
                .SingleOrDefault(_ => _.Id == consent.Id);
            
            Assert.Equal(studySubjectEntity.Id, consentEntity.StudySubject.Id);
            Assert.Equal(personId.Id, consentEntity.GivenBy.Id);
            Assert.Equal(dateGiven, consentEntity.DateProvided);
            Assert.Null(consentEntity.DateWithdrawn);
        }
        
        
        [Fact]
        public void StoresConsentCaseIdWhenAddingConsent()
        {
            var subjectIdentifier = Random.String();
            var person = createContext.Add(new PersonEntity()).Entity;
            var studySubjectEntity = createContext.Add(
                new StudySubjectEntity
                {
                    Person = person,
                    Study = createContext.Find<StudyEntity>(study.Id),
                    SubjectIdentifier = subjectIdentifier
                }).Entity;
            createContext.SaveChanges();
            
            var personId = new PersonIdentity(person.Id);
            var studySubject = new StudySubject(studyId, subjectIdentifier, personId);

            var dateGiven = Random.Date().Date;


            var pregnancyNumber = new PregnancyNumberIdentifier("6");
            var consent = CreateRepository(updateContext)
                .AddConsent(
                    new Consent(studySubject, dateGiven, personId, null, new [] { pregnancyNumber })
                );
            
            updateContext.SaveChanges();


            var caseIdentifier = readContext
                .Set<CaseIdentifierEntity>()
                .Include(_ => _.Consent)
                .SingleOrDefault(_ => _.Consent.Id == consent.Id);
                
            Assert.NotNull(caseIdentifier);
            Assert.Equal(PregnancyNumberIdentifier.TypeName, caseIdentifier.Type);
            Assert.Equal(new XmlMarshaller<PregnancyNumberIdentifier>(PregnancyNumberIdentifier.TypeName).MarshalledValue(pregnancyNumber),
                caseIdentifier.Value);
            Assert.NotNull(caseIdentifier.Consent);
            
        }
        
        [Fact]
        public void StoresConsentGivenEvidenceWhenAddingConsent()
        {
            var subjectIdentifier = Random.String();
            var person = createContext.Add(new PersonEntity()).Entity;
            var studySubjectEntity = createContext.Add(
                new StudySubjectEntity
                {
                    Person = person,
                    Study = createContext.Find<StudyEntity>(study.Id),
                    SubjectIdentifier = subjectIdentifier
                }).Entity;
            createContext.SaveChanges();
            
            var personId = new PersonIdentity(person.Id);
            var studySubject = new StudySubject(studyId, subjectIdentifier, personId);

            var dateGiven = Random.Date().Date;


            var pregnancyNumber = new PregnancyNumberIdentifier("6");
            var evidence = new MedwayEvidence { CompetentStatus = "alan", ConsentGivenBy = "phil", ConsentTakenBy = "dawn" };
            var consent = CreateRepository(updateContext)
                .AddConsent(
                    new Consent(studySubject, dateGiven, personId, new [] { evidence }, new [] { pregnancyNumber })
                );
            
            updateContext.SaveChanges();


            var storedEvidence = readContext
                .Set<EvidenceEntity>()
                .Include(_ => _.Consent)
                .SingleOrDefault(_ => _.Consent.Id == consent.Id);
                
            Assert.NotNull(storedEvidence);
            Assert.IsType<GivenEvidenceEntity>(storedEvidence);
            Assert.Equal(MedwayEvidence.TypeName, storedEvidence.Type);
            Assert.Equal(new XmlMarshaller<MedwayEvidence>(MedwayEvidence.TypeName).MarshalledValue(evidence),
                storedEvidence.Value);
            Assert.NotNull(storedEvidence.Consent);
            
        }


        [Fact]
        public void CanGetStudyById()
        {
            Assert.Equal(studyId, repository.GetStudy(study.Id));
        }

        [Fact]
        public void ReturnsNullStudyForNonExistantStudy()
        {
            Assert.Null(repository.GetStudy(-1));
        }

        [Fact]
        public void CanFindStudySubjectByStudyAndSubjectIdentifier()
        {
            AssertStudySubject(consentedStudySubject, repository.FindStudySubject(studyId, consentedStudySubject.SubjectIdentifier));
        }

        [Fact]
        public void ReturnsNullStudySubjectForNonExistantStudyAndSubjectIdentifier()
        {
            Assert.Null(repository.FindStudySubject(studyId, "nope"));
        }

        [Fact]
        public void CanFindStudySubjectByStudyAndPersonId()
        {
            AssertStudySubject(consentedStudySubject, repository.FindStudySubject(studyId, consentedPersonId));
        }

        [Fact]
        public void ReturnsNullStudySubjectForNonexistantStudyAndPersonId()
        {
            Assert.Null(repository.FindStudySubject(studyId, new PersonIdentity(-consentedStudySubject.Person.Id)));
        }

        [Fact]
        public void CanFindActiveConsentWithNoIdentifiers()
        {
            Assert.Equal(
                activeConsent.Id,
                repository.FindActiveConsent(
                    new StudySubject(
                        studyId,
                        consentedStudySubject.SubjectIdentifier,
                        consentedPersonId),
                    Enumerable.Empty<CaseIdentifier>()).Id);
        }

        [Fact()]
        public void ReturnsNullConsentForAnStudySubjectWithOnlyWithdrawnConsent()
        {
            Assert.Null(
                repository.FindActiveConsent(
                    new StudySubject(studyId, "Unconsented", unconsentedPersonId),
                    Enumerable.Empty<CaseIdentifier>()));
        }

        [Fact()]
        public void ReturnsNullConsentForAnUnknownStudySubject()
        {
            Assert.Null(repository.FindActiveConsent(
                new StudySubject(studyId, "Unknown", new PersonIdentity(67)),
                Enumerable.Empty<CaseIdentifier>()));
        }

        [Fact]
        public void FindsConsentByCaseId()
        {
            var pregnancyNumber = new PregnancyNumberIdentifier("87");
            
            var marshaller = new XmlMarshaller(typeof(PregnancyNumberIdentifier), PregnancyNumberIdentifier.TypeName);
            createContext.Set<CaseIdentifierEntity>().Add(
                new CaseIdentifierEntity
                {
                    Consent = activeConsent,
                    Type = marshaller.ValueType,
                    Value = marshaller.MarshalledValue(pregnancyNumber)
                });
            createContext.SaveChanges();


            Assert.NotNull(
                repository.FindActiveConsent(
                    new StudySubject(
                        studyId,
                        consentedStudySubject.SubjectIdentifier,
                        consentedPersonId),
                    new[] {pregnancyNumber}
                ));

            Assert.Null(
                repository.FindActiveConsent(
                    new StudySubject(
                        studyId,
                        consentedStudySubject.SubjectIdentifier,
                        consentedPersonId),
                    Enumerable.Empty<CaseIdentifier>()
                )
            );

        }

        private void AssertStudySubject(StudySubjectEntity expected, StudySubject actual)
        {
            Assert.Equal(expected.Study.Id, actual.StudyId);
            Assert.Equal(expected.Person.Id, actual.PersonId.Id);
            Assert.Equal(expected.SubjectIdentifier, actual.SubjectIdentifier);
        }
    }
    
    
}