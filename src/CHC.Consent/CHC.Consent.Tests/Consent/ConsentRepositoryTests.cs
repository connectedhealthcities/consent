using CHC.Consent.Common.Consent;
using CHC.Consent.Testing.Utils;
using FakeItEasy;
using Xunit;

namespace CHC.Consent.Tests.Consent
{
    using Consent = Common.Consent.Consent;
    public class ConsentRepositoryTests
    {
        private readonly Study study;
        private readonly ConsentRepository repository;
        private readonly StudySubject consentedStudySubject;
        private readonly Consent withdrawnConsent;
        private readonly Consent activeConsent;
        private readonly StudySubject unconsentedStudySubect;

        /// <inheritdoc />
        public ConsentRepositoryTests()
        {
            study = Create.Study.WithId(32);
            consentedStudySubject = Create.StudySubject.WithStudy(study).WithSubjectIdentifier("Id").WithSubjectPersonId(495);
            unconsentedStudySubect = Create.StudySubject.WithStudy(study)
                .WithSubjectIdentifier("Unconsented").WithSubjectPersonId(1488);
            withdrawnConsent = Create.Consent.WithStudySubject(consentedStudySubject).GivenOn(1.December(1965)).Withdrawn(1.January(1966));
            activeConsent = Create.Consent.WithStudySubject(consentedStudySubject).GivenOn(1.February(1966));
            
            repository = new ConsentRepository(
                Create.AStore(study, Create.Study),
                Create.AStore(consentedStudySubject, unconsentedStudySubect),
                Create.AStore(
                    withdrawnConsent,
                    activeConsent,
                    Create.Consent.WithStudySubject(unconsentedStudySubect).Withdrawn())
            );
        }


        [Fact]
        public void CanGetStudyById()
        {
            Assert.Equal(study, repository.GetStudy(study.Id));
        }

        [Fact]
        public void ReturnsNullStudyForNonExistantStudy()
        {
            Assert.Null(repository.GetStudy(-1));
        }

        [Fact]
        public void CanFindStudySubjectByStudyAndSubjectIdentifier()
        {
            Assert.Equal(consentedStudySubject, repository.FindStudySubject(study, consentedStudySubject.SubjectIdentifier));
        }

        [Fact]
        public void ReturnsNullStudySubjectForNonExistantStudyAndSubjectIdentifier()
        {
            Assert.Null(repository.FindStudySubject(study, "nope"));
        }

        [Fact]
        public void CanFindStudySubjectByStudyAndPersonId()
        {
            Assert.Equal(consentedStudySubject, repository.FindStudySubject(study, consentedStudySubject.PersonId));
        }
        
        [Fact]
        public void ReturnsNullStudySubjectForNonexistantStudyAndPersonId()
        {
            Assert.Null(repository.FindStudySubject(study, -consentedStudySubject.PersonId));
        }

        [Fact]
        public void CanFindActiveConsentWithNoIdentifiers()
        {
            Assert.Equal(activeConsent, repository.FindActiveConsent(consentedStudySubject));
        }
        
        [Fact]
        public void ReturnsNullConsentForAnStudySubjectWithOnlyWithdrawnConsent()
        {
            Assert.Null(repository.FindActiveConsent(unconsentedStudySubect));
        }
        
        [Fact]
        public void ReturnsNullConsentForAnUnknownStudySubject()
        {
            Assert.Null(repository.FindActiveConsent(Create.StudySubject));
        }
    }
    
    
}