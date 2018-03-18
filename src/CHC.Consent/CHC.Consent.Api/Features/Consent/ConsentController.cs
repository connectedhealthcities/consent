using System;
using System.Net;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CHC.Consent.Api.Features.Consent
{
    using Consent = Common.Consent.Consent;
    [Route("/consent")]
    public class ConsentController : Controller
    {
        private ILogger Logger { get; }
        private readonly IConsentRepository consentRepository;

        
        public ConsentController(IConsentRepository consentRepository, ILogger<ConsentController> logger=null)
        {
            Logger = logger ?? NullLogger<ConsentController>.Instance;
            this.consentRepository = consentRepository;
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
                Logger.LogWarning("Study#{studyId} not found", studyId);
                return NotFound();
            }
            var studySubject = consentRepository.FindStudySubject(studyId, specification.SubjectIdentifier);
            if (studySubject == null)
            {
                var subjectSpecification = new { specification.StudyId, specification.PersonId };
                Logger.LogDebug("No existing studySubject - creating a new subject for {spec}", subjectSpecification);
                var personId = new PersonIdentity( specification.PersonId );
                
                studySubject = consentRepository.FindStudySubject(studyId, personId );
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
                var existingConsent = consentRepository.FindActiveConsent(
                    studySubject,
                    specification.CaseId ?? Array.Empty<CaseIdentifier>());
                if (existingConsent != null)
                {
                    //TODO: Decide what to do with evidence, etc, for existing consents, or if you can be consented twice
                    return new SeeOtherOjectActionResult("Get", routeValues:new {id = existingConsent.Id }, result:existingConsent.Id);

                }
            }

            var newConsentId = consentRepository.AddConsent(
                new Consent(
                    studySubject,
                    specification.DateGiven,
                    specification.GivenBy,
                    specification.Evidence,
                    specification.CaseId));

            return CreatedAtAction("Get", new {id = newConsentId.Id }, newConsentId.Id);
        }

        [HttpGet, Route("{id}")]
        public IActionResult Get(long id)
        {
            throw new System.NotImplementedException();
        }
    }
}