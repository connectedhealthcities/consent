using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Consent.Evidences;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CHC.Consent.Api.Features.Consent
{
    [Route("/consent")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ConsentController : Controller
    {
        public EvidenceDefinitionRegistry Registry { get; }
        private ILogger Logger { get; }
        public EvidenceDtosIdentifierDtoMarshaller IdentifierDtoMarshallers { get; set; }
        private readonly IConsentRepository consentRepository;

        
        public ConsentController(
            IConsentRepository consentRepository, 
            EvidenceDefinitionRegistry registry,
            ILogger<ConsentController> logger=null)
        {
            Registry = registry;
            Logger = logger ?? NullLogger<ConsentController>.Instance;
            this.consentRepository = consentRepository;
            IdentifierDtoMarshallers = new EvidenceDtosIdentifierDtoMarshaller(registry);
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
                    studySubject);
                if (existingConsent != null)
                {
                    //TODO: Decide what to do with evidence, etc, for existing consents, or if you can be consented twice
                    return new SeeOtherOjectActionResult("Get", routeValues:new {id = existingConsent.Id }, result:existingConsent.Id);

                }
            }

            var evidence = IdentifierDtoMarshallers.ConvertToIdentifiers(specification.Evidence);
            
            var newConsentId = consentRepository.AddConsent(
                new Common.Consent.Consent(
                    studySubject,
                    specification.DateGiven,
                    specification.GivenBy,
                    evidence));

            return CreatedAtAction("Get", new {id = newConsentId.Id }, newConsentId.Id);
        }

        [HttpGet,Route("{studyId}")]
        [ProducesResponseType((int) HttpStatusCode.OK, Type = typeof(IEnumerator<string>))]
        public IActionResult Get(long studyId)
        {
            return Ok(consentRepository.GetConsentedSubjects(new StudyIdentity(studyId)).Select(_ => _.SubjectIdentifier));
        }
    }
}