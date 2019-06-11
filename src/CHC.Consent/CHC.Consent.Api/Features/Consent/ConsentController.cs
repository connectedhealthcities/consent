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
        private ILogger Logger { get; }
        private EvidenceDtosIdentifierDtoMarshaller EvidenceDtoMarshallers { get; }
        private readonly IConsentRepository consentRepository;

        
        public ConsentController(
            IConsentRepository consentRepository, 
            EvidenceDefinitionRegistry registry,
            ILogger<ConsentController> logger=null)
        {
            Logger = logger ?? NullLogger<ConsentController>.Instance;
            this.consentRepository = consentRepository;
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
            

            var studyId = consentRepository.GetStudy(specification.StudyId);
            if (studyId == null)
            {
                Logger.LogWarning("Study#{studyId} not found", specification.StudyId);
                return NotFound();
            }
            var studySubject = consentRepository.FindStudySubject(studyId, specification.SubjectIdentifier);
            if (studySubject == null)
            {
                var subjectSpecification = new { specification.StudyId, specification.PersonId };
                Logger.LogDebug("No existing studySubject - creating a new subject for {spec}", subjectSpecification);
                var personId = new PersonIdentity( specification.PersonId );
                
                studySubject = consentRepository.FindStudySubject(studyId, personId);
                if (studySubject != null)
                {
                    Logger.LogError("There is already a study subject for {spec} - {identifier}", subjectSpecification, studySubject.SubjectIdentifier);
                    return BadRequest();
                }

                studySubject = new StudySubject(studyId, specification.SubjectIdentifier, personId );
                studySubject = consentRepository.AddStudySubject(studySubject);
            }
            else
            {
                var existingConsent = consentRepository.FindActiveConsent(studySubject);
                if (existingConsent != null)
                {
                    //TODO: Decide what to do with evidence, etc, for existing consents, or if you can be consented twice
                    return new SeeOtherOjectActionResult(
                        "GetStudySubject",
                        routeValues: new {studyId, subjectIdentifier = studySubject.SubjectIdentifier},
                        result: existingConsent.Id);
                }
            }

            var evidence = EvidenceDtoMarshallers.ConvertToIdentifiers(specification.Evidence);
            
            var newConsentId = consentRepository.AddConsent(
                new Common.Consent.Consent(
                    studySubject,
                    specification.DateGiven,
                    specification.GivenBy,
                    evidence));

            return CreatedAtAction(
                "GetStudySubject",
                new {studyId, subjectIdentifier = studySubject.SubjectIdentifier},
                newConsentId.Id);
        }

        [HttpGet("{studyId}", Name="GetConsentedSubjectsForStudy")]
        [ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(IEnumerable<StudySubject>))]
        [ProducesResponseType((int) HttpStatusCode.NotFound)]
        public IActionResult GetConsentedSubjectsForStudy([BindRequired]long studyId)
        {
            var (studyIdentity, actionResult) = GetStudy(studyId);
            if (actionResult != null) return actionResult;

            return Ok(consentRepository.GetConsentedSubjects(studyIdentity));
        }

        private (StudyIdentity studyIdentity, IActionResult actionResult) GetStudy(long studyId)
        {
            var studyIdentity = consentRepository.GetStudy(studyId);
            if (studyIdentity != null) return (studyIdentity, null);
            Logger.LogWarning("Study#{studyId} not found", studyId);
            return (studyIdentity, NotFound());

        }

        [HttpGet("{studyId}/all", Name = "GetSubjectsForStudy")]
        [ProducesResponseTypeAttribute(HttpStatusCode.OK, typeof(SubjectWithWithdrawalDate[]))]
        public IActionResult GetSubjectsForStudy([BindRequired, FromRoute] long studyId)
        {
            var (studyIdentity, actionResult) = GetStudy(studyId);
            if (actionResult != null) return actionResult;

            return
                Ok(
                    consentRepository.GetSubjectsWithLastWithdrawalDate(studyIdentity)
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