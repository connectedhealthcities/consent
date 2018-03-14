using CHC.Consent.Common;
using CHC.Consent.EFCore;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.Testing.Utils;
using FakeItEasy;
using FakeItEasy.Sdk;
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
            
            var storeProvider = (IStoreProvider)new ContextStoreProvider (readContext);
            repository = new ConsentRepository(
                storeProvider.Get<StudyEntity>(),
                storeProvider.Get<StudySubjectEntity>(),
                A.Dummy<IStore<Consent>>());

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