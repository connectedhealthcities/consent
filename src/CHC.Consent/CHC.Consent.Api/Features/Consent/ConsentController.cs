using System;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Features.Consent
{
    using Consent = Common.Consent.Consent;
    [Route("/consent")]
    public class ConsentController : Controller
    {
        private readonly IConsentRepository consentRepository;

        public ConsentController(IConsentRepository consentRepository)
        {
            this.consentRepository = consentRepository;
        }

        [HttpPut]
        public IActionResult PutConsent([FromBody]ConsentSpecification specification)
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            
            

            var studyId = consentRepository.GetStudy(specification.StudyId);
            if (studyId == null)
            {
                return NotFound();
            }
            var studySubject = consentRepository.FindStudySubject(studyId, specification.SubjectIdentifier);
            if (studySubject == null)
            {
                var personId = new PersonIdentity( specification.PersonId );
                
                studySubject = consentRepository.FindStudySubject(studyId, personId );
                if (studySubject != null) return BadRequest();

                studySubject = new StudySubject(studyId, specification.SubjectIdentifier, personId );
                consentRepository.AddStudySubject(studySubject);
            }
            else
            {
                var existingConsent = consentRepository.FindActiveConsent(
                    studySubject,
                    specification.CaseId ?? Array.Empty<ConsentIdentifier>());
                if (existingConsent != null)
                {
                    //TODO: Decide what to do with evidence, etc, for existing consents, or if you can be consented twice
                    return RedirectToAction("Get", new {id = 0});

                }
            }

            consentRepository.AddConsent(
                studySubject,
                new Consent(
                    studySubject,
                    specification.DateGiven,
                    specification.GivenBy,
                    specification.Evidence,
                    specification.CaseId));

            return CreatedAtAction("Get", new {id = 0}, null);
        }

        [HttpGet, Route("{id}")]
        public IActionResult Get(long id)
        {
            throw new System.NotImplementedException();
        }
    }
}