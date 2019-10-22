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
        public IStudySubjectRepository Subjects { get; }

        /// <inheritdoc />
        public StudySubjectController(IStudySubjectRepository subjects)
        {
            Subjects = subjects;
        }

        [HttpGet,Route("byId/{stubjectId}")]
        [ProducesResponseType(typeof(StudySubject), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult Get(long studyId, string subjectIdentifier)
        {
            var studySubject = Subjects.GetStudySubject(new StudyIdentity(studyId), subjectIdentifier);
            
            if (studySubject == null)
                return NotFound();
            
            return Ok(studySubject);
        }

        [HttpGet]
        [ProducesResponseType(typeof(StudySubject), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult FindBySubjectId(long studyId, [FromQuery,Required]long personId)
        {
            var found = Subjects.FindStudySubject(new StudyIdentity(studyId), new PersonIdentity(personId));

            if (found == null) return NotFound();

            return Ok(found);
        }
    }
}