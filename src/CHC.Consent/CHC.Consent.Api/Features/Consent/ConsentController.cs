using CHC.Consent.Common.Consent;
using CHC.Consent.Common.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Features.Consent
{
    using Consent = Common.Consent.Consent;
    [Route("/consent")]
    public class ConsentController : Controller
    {
        private IConsentRepository consentRepository;

        public ConsentController(IConsentRepository consentRepository)
        {
            this.consentRepository = consentRepository;
        }

        [HttpPut]
        public IActionResult PutConsent([FromBody]ConsentSpecification specification)
        {
            if (!ModelState.IsValid) return new BadRequestObjectResult(ModelState);

            var study = consentRepository.GetStudy(specification.StudyId);
            if (study == null)
            {
                return NotFound();
            }
            var studySubject = consentRepository.FindStudySubject(study, specification.SubjectIdentifier);
            if (studySubject == null)
            {
                studySubject = consentRepository.FindStudySubject(study, specification.PersonId);
                if (studySubject != null) return BadRequest();

                studySubject = new StudySubject(study, specification.SubjectIdentifier, specification.PersonId);
                consentRepository.AddStudySubject(studySubject);
            }
            

            consentRepository.AddConsent(
                new Consent(
                    studySubject,
                    specification.DateGiven,
                    specification.Evidence));

            return CreatedAtAction("Get", new {id = 0}, null);
        }

        public IActionResult Get(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}