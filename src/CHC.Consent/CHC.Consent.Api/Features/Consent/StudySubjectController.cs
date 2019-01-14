using System.ComponentModel.DataAnnotations;
using System.Net;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Features.Consent
{
    [Route("studies/{studyId:long}/subjects")]
    public class StudySubjectController : Controller
    {
        private IConsentRepository ConsentRepository { get; }

        /// <inheritdoc />
        public StudySubjectController(IConsentRepository consentRepository)
        {
            ConsentRepository = consentRepository;
        }

        [HttpGet,Route("byId/{stubjectId}")]
        [ProducesResponseType(typeof(StudySubject), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult Get(long studyId, string subjectIdentifier)
        {
            var studySubject = ConsentRepository.FindStudySubject(new StudyIdentity(studyId), subjectIdentifier);
            
            if (studySubject == null)
                return NotFound();
            
            return Ok(studySubject);
        }

        [HttpGet]
        [ProducesResponseType(typeof(StudySubject), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult FindBySubjectId(long studyId, [FromQuery,Required]long personId)
        {
            var found = ConsentRepository.FindStudySubject(new StudyIdentity(studyId), new PersonIdentity(personId));

            if (found == null) return NotFound();

            return Ok(found);
        }
    }
}