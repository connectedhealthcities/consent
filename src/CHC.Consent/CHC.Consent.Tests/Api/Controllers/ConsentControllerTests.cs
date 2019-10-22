using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Features.Consent;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Testing.Utils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Random = CHC.Consent.Testing.Utils.Random;

namespace CHC.Consent.Tests.Api.Controllers
{
    using Consent = Common.Consent.Consent;
    
    public class ConsentControllerTests 
    {
        public class ConsentControllerTestBase
        {
            protected IActionResult Result;
            protected readonly Study Study;
            protected readonly StudySubject StudySubject;
            protected Consent CreatedConsent;
            protected readonly IConsentRepository Consents;
            protected readonly IStudyRepository Studies;
            protected readonly IStudySubjectRepository StudySubjects;
            public StudyIdentity StudyId { get; }

            /// <inheritdoc />
            public ConsentControllerTestBase()
            {
                Study = Create.Study;
                StudyId = new StudyIdentity(Study.Id);
                StudySubject = new StudySubject(StudyId, "AA100023", new PersonIdentity(500L));
                Studies = CreateStudyRepository(Study);
                StudySubjects = CreateSubjectRepository(Study, StudySubject);
                Consents = CreateConsentRepository(Study, StudySubject);
            }

            private IStudyRepository CreateStudyRepository(Study study)
            {
                var fake = A.Fake<IStudyRepository>(x => x.Strict());
                A.CallTo(() => fake.GetStudy(study.Id)).Returns(study);
                return fake;
            }

            private IStudySubjectRepository CreateSubjectRepository(Study study, StudySubject subject)
            {
                var fake = A.Fake<IStudySubjectRepository>(x => x.Strict());
                A.CallTo(() => fake.GetStudySubject(study.Id, subject.SubjectIdentifier)).Returns(subject);
                A.CallTo(() => fake.FindStudySubject(study.Id, subject.PersonId)).Returns(subject);
                return fake;
            }

            private IConsentRepository CreateConsentRepository(Study study, StudySubject studySubject)
            {
                var consentRepository = A.Fake<IConsentRepository>(x => x.Strict());
                A.CallTo(() => consentRepository.AddConsent(A<Consent>._))
                    .Invokes((Consent c) => CreatedConsent = c);
                return consentRepository;
            }

            protected void RecordConsent(
                IEnumerable<IIdentifierValueDto> evidence,
                DateTime dateGiven,
                long? givenByPersonId = null,
                IConsentRepository consentRepository = null,
                long? studyId = null,
                string subjectIdentifier = null,
                long? personId = null
                
            )
            {
                Result = CreateConsentController(consentRepository??Consents)
                    .PutConsent(
                        new ConsentSpecification
                        {
                            StudyId = studyId??Study.Id,
                            SubjectIdentifier = subjectIdentifier??StudySubject.SubjectIdentifier,
                            GivenBy = givenByPersonId??StudySubject.PersonId,
                            PersonId = personId??StudySubject.PersonId,
                            Evidence = evidence.ToArray(),
                            DateGiven = dateGiven
                        });
            }

            protected void RecordConsent(
                IIdentifierValueDto evidence,
                DateTime dateGiven,
                long? givenByPersonId = null,
                IConsentRepository consentRepository = null,
                long? studyId = null,
                string subjectIdentifier = null,
                long? personId = null
            ) =>
                RecordConsent(
                    new[] {evidence},
                    dateGiven,
                    givenByPersonId,
                    consentRepository,
                    studyId,
                    subjectIdentifier,
                    personId);

            public ConsentController CreateConsentController(IConsentRepository consentRepository)
            {
                return new ConsentController(
                    Studies, 
                    StudySubjects, 
                    consentRepository, 
                    KnownEvidence.Registry);
            }
        }

        public class WhenRecordingNewConsent_ForAnExistingStudySubject_WithoutActiveConsent : ConsentControllerTestBase
        {
            /// <inheritdoc />
            public WhenRecordingNewConsent_ForAnExistingStudySubject_WithoutActiveConsent()
            {
                A.CallTo(() => Consents.FindActiveConsent(StudySubject))
                    .Returns(null);
                RecordConsent(
                    Evidences.ServerMedwayDto(takenBy:"Peter Crowther"),
                    2.January(1837));
            }

            [Fact]
            public void ShouldReturnCreatedResponse()
            {
                var created = Assert.IsType<CreatedAtActionResult>(Result);
            }

            [Fact]
            public void ShouldCreateANewConsentRecord()
            {
                Assert.NotNull(CreatedConsent);
                
                Assert.Equal(StudyId, CreatedConsent.StudySubject.StudyId);
                Assert.Same(StudySubject, CreatedConsent.StudySubject);
                Assert.Equal(StudySubject.PersonId, CreatedConsent.GivenByPersonId);
                
                Assert.Equal(2.January(1837), CreatedConsent.DateGiven);
                Assert.Equal(Evidences.MedwayEvidence(takenBy:"Peter Crowther"), CreatedConsent.GivenEvidence.Single());
            }
        }

        public class WhenRecordingConsent_ForANonExistantStudy : ConsentControllerTestBase
        {
            /// <inheritdoc />
            public WhenRecordingConsent_ForANonExistantStudy()
            {
                var nonExistentStudyId = - Study.Id;

                A.CallTo(() => Studies.GetStudy(nonExistentStudyId)).Returns(null);
                
                RecordConsent(A.Dummy<IIdentifierValueDto>(), A.Dummy<DateTime>(), studyId: nonExistentStudyId);
            }

            [Fact]
            public void ShouldReturnANotFoundResult()
            {
                Assert.IsType<NotFoundResult>(Result);
            }

            [Fact]
            public void ShouldNotCreateANewConsentRecord()
            {
                Assert.Null(CreatedConsent);
            }
        }
        
        public class WhenRecordingConsent_ForANewStudySubject : ConsentControllerTestBase
        {
            private readonly string newSubjectIdentifier;
            private readonly long newPersonId;
            private StudySubject createdStudySubject;


            /// <inheritdoc />
            public WhenRecordingConsent_ForANewStudySubject()
            {
                newSubjectIdentifier = StudySubject.SubjectIdentifier + "New";
                newPersonId = StudySubject.PersonId.Id + 1;
                A.CallTo(() => StudySubjects.GetStudySubject(StudyId, newSubjectIdentifier)).Returns(null);
                A.CallTo(() => StudySubjects.FindStudySubject(StudyId, new PersonIdentity(newPersonId))).Returns(null);

                A.CallTo(() => StudySubjects.AddStudySubject(A<StudySubject>._))
                    .Invokes((StudySubject created) => createdStudySubject = created);
                
                RecordConsent(Evidences.ServerMedwayDto(takenBy:"Michael Fish"), A.Dummy<DateTime>(), subjectIdentifier: newSubjectIdentifier, personId: newPersonId);
            }

            [Fact]
            public void ShouldReturnACreatedResult()
            {
                Assert.IsType<CreatedAtActionResult>(Result);
            }

            [Fact]
            public void ShouldCreateANewConsentRecord()
            {
                Assert.NotNull(CreatedConsent);
            }

            [Fact]
            public void CreatedSubjectShouldHaveCorrectProperties()
            {
                Assert.Equal(StudyId, createdStudySubject.StudyId);
                Assert.Equal(newPersonId, createdStudySubject.PersonId);
                Assert.Equal(newSubjectIdentifier, createdStudySubject.SubjectIdentifier);
            }
        }


        public class
            WhenRecordingConsent_ForAnExistingStudySubject_WithANewSubjectIdentifier : ConsentControllerTestBase
        {
            private readonly string newSubjectIdentifier;
            
            /// <inheritdoc />
            public WhenRecordingConsent_ForAnExistingStudySubject_WithANewSubjectIdentifier()
            {
                newSubjectIdentifier = StudySubject.SubjectIdentifier + "New";
                
                A.CallTo(() => StudySubjects.GetStudySubject(StudyId, newSubjectIdentifier)).Returns(null);
                
                
                RecordConsent(A.Dummy<IIdentifierValueDto>(), A.Dummy<DateTime>(), subjectIdentifier: newSubjectIdentifier);
            }

            [Fact]
            public void ReturnsABadRequest()
            {
                Assert.IsType<BadRequestResult>(Result);
            }

            [Fact]
            public void DoesNotCreateConsent()
            {
                Assert.Null(CreatedConsent);
            }
        }

        public class WhenTryingToRecordConsent_ForAnExistingStudySubject_WithActiveConsent : ConsentControllerTestBase
        {
            private readonly ConsentIdentity existingConsentId;

            public WhenTryingToRecordConsent_ForAnExistingStudySubject_WithActiveConsent()
            {
                
                var dateGiven = 3.November(1472);
                var givenEvidence = Array.Empty<IIdentifierValueDto>();
                existingConsentId = new ConsentIdentity(43); 
                    
                A.CallTo(() => Consents.FindActiveConsent(StudySubject))
                    .Returns(existingConsentId);
                
                RecordConsent(givenEvidence, dateGiven);
            }

            [Fact]
            public void ReturnsFoundResponse()
            {
                Assert.IsType<SeeOtherOjectActionResult>(Result);
            }
            
            [Fact]
            public void DoesNotCreateConsent()
            {
                Assert.Null(CreatedConsent);
            }
        }
        
        public class WhenTryingToRecordConsent_ForAnExistingStudySubject_WithActiveConsentForDifferentIdentifiers : ConsentControllerTestBase
        {
            private readonly Consent existingConsent;
            private const string PregancyId = "1";

            public WhenTryingToRecordConsent_ForAnExistingStudySubject_WithActiveConsentForDifferentIdentifiers()
            {
                var givenEvidence = new [] { A.Dummy<Evidence>() };
                var dateGiven = 3.November(1472);
                
                existingConsent = new Consent(StudySubject, dateGiven, Random.Long(), givenEvidence);
                A.CallTo(
                        () =>
                            Consents.FindActiveConsent(
                                StudySubject))
                    .Returns(null);
                
                RecordConsent(Array.Empty<IIdentifierValueDto>(), dateGiven);
            }

            [Fact]
            public void ReturnsFoundResponse()
            {
                Assert.IsType<CreatedAtActionResult>(Result);
            }
            
            [Fact]
            public void CreatesConsent()
            {
                Assert.NotNull(CreatedConsent);
            }
        }

        public class WhenGettingConsentedSubjectIdentifiers_ForAKnownStudy_OnlyReturnsSubjectIdentifiers : ConsentControllerTestBase
        {
            private StudySubject[] studySubjects;
            private IActionResult result;

            /// <inheritdoc />
            public WhenGettingConsentedSubjectIdentifiers_ForAKnownStudy_OnlyReturnsSubjectIdentifiers()
            {
                studySubjects = Enumerable.Range(0, 100)
                    .Select((_, index) => 
                        new StudySubject(StudyId, Random.String(10), (PersonIdentity) (index + 25)))
                    .ToArray();
                
                
                A.CallTo(() => StudySubjects.GetConsentedSubjects(StudyId))
                    .Returns(studySubjects);

                result = CreateConsentController(Consents).GetConsentedSubjectsForStudy(StudyId);
            }

            [Fact]
            public void ReturnsOkResult() => result.Should().BeOfType<OkObjectResult>();

            [Fact]
            public void ResultContainsAllSubjectIds() => 
                result.Should().BeOfType<OkObjectResult>()
                    .Which.Value.Should().BeEquivalentTo(studySubjects);

        }

    }
}