using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Features.Consent
{
    [Route("subject-identifiers")]
    public class SubjectIdentifierController : Controller
    {
        private ISubjectIdentifierRepository SubjectIdentifierRepository { get; }

        /// <inheritdoc />
        public SubjectIdentifierController(ISubjectIdentifierRepository subjectIdentifierRepository)
        {
            SubjectIdentifierRepository = subjectIdentifierRepository;
        }

        [HttpPost("{studyId:long}")]
        [Produces(typeof(string))]
        [AutoCommit]
        public IActionResult Generate(long studyId)
        {
            return Ok(SubjectIdentifierRepository.GenerateIdentifier(new StudyIdentity(studyId)));
        }
    }
}