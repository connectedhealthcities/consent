using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CHC.Consent.Api.Features.Consent
{
    using ProducesResponseTypeAttribute = Infrastructure.Web.ProducesResponseTypeAttribute;
    [Route("/consent")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ConsentController : Controller
    {
        private IStudyRepository Studies { get; }
        private IStudySubjectRepository Subjects { get; }
        private ILogger Logger { get; }
        private EvidenceDtosIdentifierDtoMarshaller EvidenceDtoMarshallers { get; }
        private IConsentRepository Consents { get; }

        
        public ConsentController(
            IStudyRepository studies,
            IStudySubjectRepository subjects,
            IConsentRepository consents, 
            EvidenceDefinitionRegistry registry,
            ILogger<ConsentController> logger=null)
        {
            Studies = studies;
            Subjects = subjects;
            Logger = logger ?? NullLogger<ConsentController>.Instance;
            Consents = consents;
            EvidenceDtoMarshallers = new EvidenceDtosIdentifierDtoMarshaller(registry);
        }

        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.Created, Type=typeof(long))]
        [ProducesResponseType((int) HttpStatusCode.SeeOther, Type=typeof(long))]
        [ProducesResponseType((int) HttpStatusCode.BadRequest, Type=typeof(SerializableError))]
        [AutoCommit]
        public IActionResult PutConsent([FromBody]ConsentSpecification specification)
        {
            if (!ModelState.IsValid)
            {
                Logger.LogInformation("Invalid ModelState {@error}", new SerializableError(ModelState));
                return new BadRequestObjectResult(ModelState);
            }
            

            var study = Studies.GetStudy(specification.StudyId);
            if (study == null)
            {
                Logger.LogWarning("Study#{studyId} not found", specification.StudyId);
                return NotFound();
            }
            var studySubject = Subjects.GetStudySubject(study.Id, specification.SubjectIdentifier);
            if (studySubject == null)
            {
                var subjectSpecification = new { specification.StudyId, specification.PersonId };
                Logger.LogDebug("No existing studySubject - creating a new subject for {spec}", subjectSpecification);
                var personId = new PersonIdentity( specification.PersonId );
                
                studySubject = Subjects.FindStudySubject(study.Id, personId);
                if (studySubject != null)
                {
                    Logger.LogError("There is already a study subject for {spec} - {identifier}", subjectSpecification, studySubject.SubjectIdentifier);
                    return BadRequest();
                }

                studySubject = new StudySubject(study.Id, specification.SubjectIdentifier, personId );
                studySubject = Subjects.AddStudySubject(studySubject);
            }
            else
            {
                var existingConsent = Consents.FindActiveConsent(studySubject);
                if (existingConsent != null)
                {
                    //TODO: Decide what to do with evidence, etc, for existing consents, or if you can be consented twice
                    return new SeeOtherOjectActionResult(
                        "GetStudySubject",
                        routeValues: new {studyId = study, subjectIdentifier = studySubject.SubjectIdentifier},
                        result: existingConsent.Id);
                }
            }

            var evidence = EvidenceDtoMarshallers.ConvertToIdentifiers(specification.Evidence);
            
            var newConsentId = Consents.AddConsent(
                new Common.Consent.Consent(
                    studySubject,
                    specification.DateGiven,
                    specification.GivenBy,
                    evidence));

            return CreatedAtAction(
                "GetStudySubject",
                new {studyId = study, subjectIdentifier = studySubject.SubjectIdentifier},
                newConsentId.Id);
        }

        [HttpGet("{studyId}", Name="GetConsentedSubjectsForStudy")]
        [ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(IEnumerable<StudySubject>))]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        public IActionResult GetConsentedSubjectsForStudy([BindRequired]long studyId)
        {
            var (studyIdentity, actionResult) = GetStudy(studyId);
            if (actionResult != null) return actionResult;

            return Ok(Subjects.GetConsentedSubjects(studyIdentity));
        }

        private (StudyIdentity studyIdentity, IActionResult actionResult) GetStudy(long studyId)
        {
            var study = Studies.GetStudy(studyId);
            if (study != null) return (study.Id, null);
            Logger.LogWarning("Study#{studyId} not found", studyId);
            return (study.Id, NotFound());

        }

        [HttpGet("{studyId}/all", Name = "GetSubjectsForStudy")]
        [ProducesResponseTypeAttribute(HttpStatusCode.OK, typeof(SubjectWithWithdrawalDate[]))]
        public IActionResult GetSubjectsForStudy([BindRequired, FromRoute] long studyId)
        {
            var (studyIdentity, actionResult) = GetStudy(studyId);
            if (actionResult != null) return actionResult;

            return
                Ok(
                    Subjects.GetSubjectsWithLastWithdrawalDate(studyIdentity)
                        .Select(_ => 
                            new SubjectWithWithdrawalDate(_.studySubject.SubjectIdentifier, _.lastWithDrawn)
                        )
                );
        }

        [HttpGet("{studyId}/{subjectIdentifier}", Name = "GetConsentedStudySubject")]
        public IActionResult GetStudySubject(
            [BindRequired, FromRoute] long studyId,
            [BindRequired, FromRoute] string subjectIdentifier)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            throw new NotImplementedException();
        }

        public class SubjectWithWithdrawalDate
        {
            public string SubjectIdentifier { get;  }
            public DateTime? LastWithdrawalDate { get; }

            /// <inheritdoc />
            public SubjectWithWithdrawalDate(string subjectIdentifier, DateTime? lastWithdrawalDate)
            {
                SubjectIdentifier = subjectIdentifier;
                LastWithdrawalDate = lastWithdrawalDate;
            }
        }
    }
}