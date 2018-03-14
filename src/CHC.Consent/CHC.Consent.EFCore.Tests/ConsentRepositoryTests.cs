using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.EFCore;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.Testing.Utils;
using FakeItEasy;
using FakeItEasy.Sdk;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Z.EntityFramework.Plus;

namespace CHC.Consent.EFCore.Tests
{
    using Consent = Common.Consent.Consent;
    [Collection(DatabaseCollection.Name)]
    public class ConsentRepositoryTests : DbTests
    {
        private readonly StudyEntity study;
        private readonly ConsentRepository repository;
        private readonly StudySubjectEntity consentedStudySubject;
        private readonly Consent withdrawnConsent;
        private readonly Consent activeConsent;
        private readonly StudySubjectEntity unconsentedStudySubect;
        private ConsentContext readContext;
        private ConsentContext updateContext;
        private ConsentContext createContext;
        private readonly StudyIdentity studyId;
        private readonly PersonIdentity consentedPersonId;

        /// <inheritdoc />
        public ConsentRepositoryTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            readContext = CreateNewContextInSameTransaction();
            updateContext = CreateNewContextInSameTransaction();
            createContext = CreateNewContextInSameTransaction();


            study = createContext.Studies.Add(new StudyEntity{Name = Random.String()}).Entity;
            
            var consentedPerson = createContext.People.Add(new PersonEntity()).Entity;
            consentedStudySubject = createContext.Add(new StudySubjectEntity{ Study = study, Person = consentedPerson, SubjectIdentifier = "Consented"}).Entity;
            createContext.SaveChanges();
            
            studyId = new StudyIdentity(study.Id);
            consentedPersonId = consentedPerson;
            
            repository = CreateRepository(readContext);

            /*withdrawnConsent = Create.Consent.WithStudySubject(consentedStudySubject).GivenOn(1.December(1965)).Withdrawn(1.January(1966));
            activeConsent = Create.Consent.WithStudySubject(consentedStudySubject).GivenOn(1.February(1966));
            
            var unconsentedPerson = createContext.People.Add(new PersonEntity()).Entity;
            unconsentedStudySubect = createContext.Add(
                    new StudySubjectEntity().SetStudy(study).SetPerson(consentedPerson)
                        .SetSubjectIdentifier("UnConsented"))
                .Entity;
            
            
            repository = new ConsentRepository(
                Create.AStore(study, Create.Study),
                Create.AStore(consentedStudySubject, unconsentedStudySubect),
                Create.AStore(
                    withdrawnConsent,
                    activeConsent,
                    Create.Consent.WithStudySubject(unconsentedStudySubect).Withdrawn())
            );*/
        }

        private ConsentRepository CreateRepository(ConsentContext consentContext)
        {
            var storeProvider = (IStoreProvider) new ContextStoreProvider(consentContext);
            return new ConsentRepository(
                storeProvider.Get<StudyEntity>(),
                storeProvider.Get<StudySubjectEntity>(),
                storeProvider.Get<PersonEntity>(),
                storeProvider.Get<ConsentEntity>());
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
            Assert.NotEqual(0, addedSubject.Id);
            Assert.Equal(studyId, addedSubject.StudyId);
            Assert.Equal(personId, addedSubject.PersonId);
            Assert.Equal(subjectIdentifier, addedSubject.SubjectIdentifier);

            var studySubjectEntity = readContext.Set<StudySubjectEntity>().Find(addedSubject.Id);
            readContext.Entry(studySubjectEntity).Reference(_ => _.Study).Load();
            readContext.Entry(studySubjectEntity).Reference(_ => _.Person).Load();
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
            var studySubject = new StudySubject(studySubjectEntity.Id, studyId, subjectIdentifier, personId);

            var dateGiven = Random.Date().Date;


            var consent = CreateRepository(updateContext)
                .AddConsent(
                    new Consent(studySubject, dateGiven, personId, null, Enumerable.Empty<ConsentIdentifier>())
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
            Assert.Equal(null, consentEntity.DateWithdrawn);
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
            Assert.Equal(consentedStudySubject.Id, repository.FindStudySubject(studyId, consentedStudySubject.SubjectIdentifier).Id);
        }

        [Fact]
        public void ReturnsNullStudySubjectForNonExistantStudyAndSubjectIdentifier()
        {
            Assert.Null(repository.FindStudySubject(studyId, "nope"));
        }

        [Fact]
        public void CanFindStudySubjectByStudyAndPersonId()
        {
            Assert.Equal(consentedStudySubject.Id, repository.FindStudySubject(studyId, consentedPersonId).Id);
        }
        
        [Fact]
        public void ReturnsNullStudySubjectForNonexistantStudyAndPersonId()
        {
            Assert.Null(repository.FindStudySubject(studyId, new PersonIdentity(-consentedStudySubject.Person.Id)));
        }

        [Fact(Skip = "In Progress")]
        public void CanFindActiveConsentWithNoIdentifiers()
        {
            //Assert.Equal(activeConsent, repository.FindActiveConsent(consentedStudySubject));
        }
        
        [Fact(Skip = "In Progress")]
        public void ReturnsNullConsentForAnStudySubjectWithOnlyWithdrawnConsent()
        {
            //Assert.Null(repository.FindActiveConsent(unconsentedStudySubect));
        }
        
        [Fact(Skip = "In Progress")]
        public void ReturnsNullConsentForAnUnknownStudySubject()
        {
            //Assert.Null(repository.FindActiveConsent());
        }
    }
    
    
}