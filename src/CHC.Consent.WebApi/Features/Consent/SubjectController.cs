using System;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.WebApi.Abstractions;
using CHC.Consent.WebApi.Abstractions.Consent;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            }
        }

        // GET
        public IEnumerable<Responses.SubjectSummary> Index(Guid studyId)
        {
            //TODO: Does a person need access to a study to get subjects that it has access to?
            return Subjects.GetSubjects(studyId: studyId).Select(s => new Responses.SubjectSummary{ Id = s.Identifier }).ToArray();
        }

        [Route("{id}")]
        public IActionResult Post(Guid studyId, string id)
        {
            try
            {
                Subjects.AddSubject(studyId, id);   
            }
            catch (SubjectAlreadyExistsException)
            {
            }
            catch (StudyNotFoundException)
            {
                return NotFound();
            }
            catch (AccessDeniedException)
            {
                return Forbid();
            }
            return CreatedAtAction("Index", new {studyId = studyId, id = id}, null);
        }
    }
}