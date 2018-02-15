using System;
using System.Collections.Generic;
using CHC.Consent.Api.Features.Consent;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Xunit;

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
            protected readonly IConsentRepository ConsentRepository;

            /// <inheritdoc />
            public ConsentControllerTestBase()
            {
                Study = new Study("AStudy");
                StudySubject = new StudySubject(Study, "AA100023", 500L);
                ConsentRepository = CreateConsentRepository(Study, StudySubject);
            }

            

            private IConsentRepository CreateConsentRepository(Study study, StudySubject studySubject)
            {
                var consentRepository = A.Fake<IConsentRepository>(x => x.Strict());
                A.CallTo(() => consentRepository.GetStudy(study.Id)).Returns(study);
                A.CallTo(() => consentRepository.FindStudySubject(study, studySubject.SubjectIdentifier)).Returns(studySubject);
                A.CallTo(() => consentRepository.FindStudySubject(Study, StudySubject.PersonId)).Returns(studySubject);
                A.CallTo(() => consentRepository.AddConsent(A<Consent>._))
                    .Invokes((Consent c) => { CreatedConsent = c; });
                return consentRepository;
            }

            protected void RecordConsent(Evidence evidence, DateTime dateGiven, IConsentRepository consentRepository=null, string studyId=null, string subjectIdentifier=null, long? personId=null)
            {
                
                Result = CreateConsentController(consentRepository??ConsentRepository)
                    .PutConsent(
                        new ConsentSpecification
                        {
                            StudyId = studyId??Study.Id,
                            SubjectIdentifier = subjectIdentifier??StudySubject.SubjectIdentifier,
                            PersonId = personId??StudySubject.PersonId,
                            Evidence = evidence,
                            DateGiven = dateGiven
                        });
            }

            private static ConsentController CreateConsentController(IConsentRepository consentRepository)
            {
                return new ConsentController(consentRepository);
            }
        }

        public class WhenRecordingNewConsent_ForAnExistingStudySubject : ConsentControllerTestBase
        {
            /// <inheritdoc />
            public WhenRecordingNewConsent_ForAnExistingStudySubject()
            {
                RecordConsent(new MedwayEvidence {ConsentTakenBy = "Peter Crowther"}, 2.January(1837));
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
                
                Assert.Equal(Study, CreatedConsent.StudySubject.Study);
                Assert.Same(StudySubject, CreatedConsent.StudySubject);
                
                Assert.Equal(2.January(1837), CreatedConsent.DateGiven);
                Assert.Equal(new MedwayEvidence { ConsentTakenBy = "Peter Crowther"}, CreatedConsent.GivenEvidence);
            }
        }

        public class WhenRecordingConsent_ForANonExistantStudy : ConsentControllerTestBase
        {
            /// <inheritdoc />
            public WhenRecordingConsent_ForANonExistantStudy()
            {
                var nonExistentStudyId = Study.Id + "NotExists";

                A.CallTo(() => ConsentRepository.GetStudy(nonExistentStudyId)).Returns(null);
                
                RecordConsent(A.Dummy<Evidence>(), A.Dummy<DateTime>(), studyId:nonExistentStudyId);
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
                newPersonId = StudySubject.PersonId + 1;
                A.CallTo(() => ConsentRepository.FindStudySubject(Study, newSubjectIdentifier)).Returns(null);
                A.CallTo(() => ConsentRepository.FindStudySubject(Study, newPersonId)).Returns(null);

                A.CallTo(() => ConsentRepository.AddStudySubject(A<StudySubject>._))
                    .Invokes((StudySubject created) => createdStudySubject = created);
                
                RecordConsent(A.Dummy<Evidence>(), A.Dummy<DateTime>(), subjectIdentifier:newSubjectIdentifier, personId:newPersonId);
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
                Assert.Equal(Study, createdStudySubject.Study);
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
                
                A.CallTo(() => ConsentRepository.FindStudySubject(Study, newSubjectIdentifier)).Returns(null);
                
                
                RecordConsent(A.Dummy<Evidence>(), A.Dummy<DateTime>(), subjectIdentifier:newSubjectIdentifier);
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
            private readonly Consent existingConsent;

            public WhenTryingToRecordConsent_ForAnExistingStudySubject_WithActiveConsent()
            {
                var givenEvidence = A.Dummy<Evidence>();
                var dateGiven = 3.November(1472);
                existingConsent = new Consent(StudySubject, dateGiven, givenEvidence);
                A.CallTo(() => ConsentRepository.FindActiveConsent(StudySubject, A<IEnumerable<Identifier>>.That.IsEmpty()))
                    .Returns(existingConsent);
                
                RecordConsent(givenEvidence, dateGiven);
            }

            [Fact]
            public void ReturnsFoundResponse()
            {
                Assert.IsType<RedirectToActionResult>(Result);
            }
            
            [Fact]
            public void DoesNotCreateConsent()
            {
                Assert.Null(CreatedConsent);
            }
        }

    }
}