using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CHC.Consent.WebApi.Abstractions;
using CHC.Consent.WebApi.Abstractions.Consent;
using CHC.Consent.WebApi.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Version = CHC.Consent.WebApi.Infrastructure.Version;

namespace CHC.Consent.WebApi.Features.Consent
{
    [Authorize]
    [Version._0_1_Dev]
    [Route("/v{version:apiVersion}/study/{studyId:guid}/subjects/")]
    public class SubjectController : Controller
    {
        public ISubjectStore Subjects { get; }

        public SubjectController(ISubjectStore subjects)
        {
            Subjects = subjects;
        }

        public static class Responses
        {
            public class SubjectSummary
            {
                /// <summary>
                /// the subject identifier
                /// </summary>
                public string Id { get; set; }
                public bool? HasConsent { get; set; }
            }

            public class ConsentCreated
            {
                public Guid StudyId { get; set; }
                public string SubjectId { get; set; }
                public DateTimeOffset WhenGiven { get; set; }
            }
        }

        public static class Requests
        {
            public class Consent
            {
                [Required]
                public DateTimeOffset WhenGiven { get; set; }
                [Required]
                public string[] Evidence { get; set; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="studyId"></param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Responses.SubjectSummary> Index(Guid studyId)
        {
            //TODO: Does a person need access to a study to get subjects that it has access to?
            return Subjects.GetSubjects(studyId).Select(s => new Responses.SubjectSummary{ Id = s.Identifier, HasConsent = s.Consents.Any(_ => _.DateWithdrawlRecorded == null)}).ToArray();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Responses.SubjectSummary), 200)]
        public IActionResult Index(Guid studyId, string id)
        {
            var subject = Subjects.GetSubjects(studyId)
                .Where(_ => _.Identifier == id)
                .Select(s => new Responses.SubjectSummary{ Id = s.Identifier, HasConsent = s.Consents.Any(_ => _.DateWithdrawlRecorded == null)})
                .SingleOrDefault();
            if (subject == null) return NotFound();
            return Ok(subject);
        }

        [HttpPost("{id}")]
        public IActionResult Post(Guid studyId, string id)
        {
            try
            {
                Subjects.AddSubject(studyId, id);   
            }
            catch (SubjectAlreadyExistsException)
            {
                return new ConflictResult("Index", routeValues: new { studyId, id });
            }
            catch (StudyNotFoundException)
            {
                return NotFound();
            }
            catch (AccessDeniedException)
            {
                return Forbid();
            }
            return CreatedAtAction("Index", new {studyId, id}, null);
        }

        [HttpPost("{id}/consent")]
        [ProducesResponseType(typeof(Responses.ConsentCreated), 200)]
        public IActionResult Post(Guid studyId, string id, [FromBody,Required]Requests.Consent consent)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            try
            {
                var recordedConsent = Subjects.AddConsent(studyId, id, consent.WhenGiven, consent.Evidence);

                return Ok(
                    new Responses.ConsentCreated
                    {
                        StudyId = studyId,
                        SubjectId = id,
                        WhenGiven = recordedConsent.DateProvisionRecorded
                    });
            }
            catch (SubjectNotFoundException)
            {
                return new NotFoundResult();
            }
            catch (NotImplementedException)
            {
                return new BadRequestResult();
            }
            catch (AccessDeniedException)
            {
                return Forbid();
            }
        }
    }
}