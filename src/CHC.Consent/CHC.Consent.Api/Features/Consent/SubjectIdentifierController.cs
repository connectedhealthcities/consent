using System.Net;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Consent;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Features.Consent
{
    using ProducesResponseTypeAttribute = Infrastructure.Web.ProducesResponseTypeAttribute;
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
        [ProducesResponseType(typeof(StudySubjectValue), (int)HttpStatusCode.OK)]
        [AutoCommit]
        public IActionResult Generate(long studyId)
        {
            return Ok(
                new StudySubjectValue
                {
                    Value = SubjectIdentifierRepository.GenerateIdentifier(new StudyIdentity(studyId))
                });
        }

        
    }

    public class StudySubjectValue
    {
        public string Value { get; set; }
    }
}