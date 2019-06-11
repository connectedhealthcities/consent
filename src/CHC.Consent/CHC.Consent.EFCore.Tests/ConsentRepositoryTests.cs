using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.Testing.Utils;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Random = CHC.Consent.Testing.Utils.Random;

namespace CHC.Consent.EFCore.Tests
{
    [Collection(DatabaseCollection.Name)]
    public class ConsentRepositoryTests : DbTests
    {
        private readonly StudyEntity study;
        private readonly ConsentRepository repository;
        private readonly StudySubjectEntity consentedStudySubject;
        
        private readonly ConsentEntity activeConsent;
        
        private readonly StudyIdentity studyId;
        private readonly PersonIdentity consentedPersonId;
        private readonly PersonIdentity unconsentedPersonId;
        private readonly PersonIdentity withdrawnPersonId;
        private DateTime withdrawnSubjectWithdrawnDate;

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

            var withdrawnConsent = createContext.Add(new PersonEntity()).Entity;
            var withdrawnSubject = createContext.Add(
                new StudySubjectEntity {Study = study, Person = withdrawnConsent, SubjectIdentifier = "Withdrawn"}).Entity;
            withdrawnSubjectWithdrawnDate = 5.January(2018);
            createContext.Add(new ConsentEntity
            {
                StudySubject = withdrawnSubject,
                DateProvided = 1.December(2017),
                DateWithdrawn = withdrawnSubjectWithdrawnDate,
                GivenBy = withdrawnConsent
            });
            
            
            createContext.SaveChanges();
            
            studyId = new StudyIdentity(study.Id);
            consentedPersonId = consentedPerson;
            unconsentedPersonId = unconsentedPerson;
            withdrawnPersonId = withdrawnConsent;
            
            repository = CreateRepository(readContext);

            
            
           
        }

        private ConsentRepository CreateRepository(
            ConsentContext consentContext, 
            params EvidenceDefinition[] evidentTypes)
        {
            var storeProvider = (IStoreProvider) new ContextStoreProvider(consentContext);


            var evidenceRegistry =
                evidentTypes.Any() ? new EvidenceDefinitionRegistry(evidentTypes) : KnownEvidence.Registry;
            
            return new ConsentRepository(
                storeProvider.Get<StudyEntity>(),
                storeProvider.Get<StudySubjectEntity>(),
                storeProvider.Get<PersonEntity>(),
                storeProvider.Get<ConsentEntity>(),
                storeProvider.Get<EvidenceEntity>(),
                evidenceRegistry
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

            using (new AssertionScope())
            {
                addedSubject.Should().NotBeNull();
                addedSubject.StudyId.Should().Be(studyId);
                addedSubject.PersonId.Should().Be(personId);
                addedSubject.SubjectIdentifier.Should().Be(subjectIdentifier);
            }

            var studySubjectEntity = readContext.Set<StudySubjectEntity>()
                .Include(_ => _.Study)
                .Include(_ => _.Person)
                .SingleOrDefault(
                _ => _.Study.Id == addedSubject.StudyId && _.Person.Id == addedSubject.PersonId);

            using (new AssertionScope())
            {
                studySubjectEntity.Should().NotBeNull();
                studySubjectEntity.SubjectIdentifier.Should().Be(subjectIdentifier);
                studySubjectEntity.Study.Id.Should().Be(studyId.Id);
                studySubjectEntity.Person.Id.Should().Be(personId.Id);
            }
        }

        [Fact]
        public void StoresConsentHeaderWhenAddingConsent()
        {
            var subjectIdentifier = Random.String();
            var (personId, studySubjectEntity, studySubject) = CreateStudySubject(subjectIdentifier);

            var dateGiven = Random.Date().Date;


            var consent = CreateRepository(updateContext)
                .AddConsent(
                    new Common.Consent.Consent(studySubject, dateGiven, personId, null)
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

        private (PersonIdentity personId, StudySubjectEntity studySubjectEntity, StudySubject studySubject) 
            CreateStudySubject(string subjectIdentifier)
        {
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
            return (personId, studySubjectEntity, studySubject);
        }


        [Fact]
        public void StoresConsentGivenEvidenceWhenAddingConsent()
        {
            var subjectIdentifier = Random.String();
            var (personId, _, studySubject) = CreateStudySubject(subjectIdentifier);

            var dateGiven = Random.Date().Date;


            var evidence = Evidences.MedwayEvidence(competencyStatus: "Competent", takenBy: "Nurse Randall");
            var marshalledEvidence =
                new CompositeIdentifierXmlMarshaller<Evidence, EvidenceDefinition>(KnownEvidence.Medway)
                    .MarshallToXml(evidence)
                    .ToString(SaveOptions.DisableFormatting);
            
            var consent = CreateRepository(updateContext)
                .AddConsent(
                    new Common.Consent.Consent(studySubject, dateGiven, personId, new [] { evidence })
                );
            
            updateContext.SaveChanges();


            var savedConsent = readContext.Set<ConsentEntity>().AsNoTracking()
                .Where(_ => _.Id == consent.Id)
                .Include(_ => _.GivenEvidence)
                .SingleOrDefault();

            savedConsent.Should().NotBeNull();
            savedConsent.GivenEvidence.Should().ContainSingle();
            var storedEvidence = savedConsent.GivenEvidence.SingleOrDefault();
            storedEvidence.Should().NotBeNull().And.BeOfType<GivenEvidenceEntity>();
            
            Assert.Equal(KnownEvidence.Medway.SystemName, storedEvidence.Type);
            
            Assert.Equal(marshalledEvidence,
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
            repository.FindStudySubject(studyId, "nope").Should().BeNull();
        }

        [Fact]
        public void CanFindStudySubjectByStudyAndPersonId()
        {
            AssertStudySubject(consentedStudySubject, repository.FindStudySubject(studyId, consentedPersonId));
        }

        [Fact]
        public void ReturnsNullStudySubjectForNonExistantStudyAndPersonId()
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
                        consentedPersonId)).Id);
        }

        [Fact()]
        public void ReturnsNullConsentForAnStudySubjectWithOnlyWithdrawnConsent()
        {
            Assert.Null(
                repository.FindActiveConsent(
                    new StudySubject(studyId, "Unconsented", unconsentedPersonId)));
        }

        [Fact()]
        public void ReturnsNullConsentForAnUnknownStudySubject()
        {
            Assert.Null(repository.FindActiveConsent(
                new StudySubject(studyId, "Unknown", new PersonIdentity(67))));
        }

        [Fact]
        public void OnlyGetsConsentedSubjectsForStudy()
        {
            repository.GetConsentedSubjects(studyId)
                .Should()
                .OnlyContain(_ => _.StudyId == studyId && _.PersonId == consentedPersonId && _.SubjectIdentifier == "Consented");
        }

        [Fact]
        public void GetsLatestWithdrawnDateForStudySubjects()
        {
            repository.GetSubjectsWithLastWithdrawalDate(studyId)
                .Should()
                .HaveCount(2)
                .And
                .ContainSingle(_ => _.studySubject.PersonId.Id == consentedPersonId.Id && _.lastWithDrawn == null)
                .And
                .ContainSingle(
                    _ => _.studySubject.PersonId == withdrawnPersonId.Id &&
                         _.lastWithDrawn == withdrawnSubjectWithdrawnDate);
        }

        private static void AssertStudySubject(StudySubjectEntity expected, StudySubject actual)
        {
            using (new AssertionScope())
            {
                actual.StudyId.Id.Should().Be(expected.Study.Id);
                actual.PersonId.Id.Should().Be(expected.Person.Id);
                actual.SubjectIdentifier.Should().Be(expected.SubjectIdentifier);    
            }
        }
    }
    
    
}