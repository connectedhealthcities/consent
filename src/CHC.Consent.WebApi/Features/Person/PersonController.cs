using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CHC.Consent.WebApi.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.WebApi.Features.Person
{
    /// <summary>
    /// Provides access to People (subjects) in the system 
    /// </summary>
    [Authorize]
    [Version._0_1_Dev]
    [Route("/v{version:apiVersion}/person")]
    public class PersonController : Controller
    {
        /// <inheritdoc />
        public PersonController()
        {
        }

        [HttpGet]
        public IEnumerable<ResponseModels.Person> Get()
        {
            var issuer = User.FindFirstValue("iss");
            var subject = User.FindFirstValue("sub");
            User.IsInRole("study_administrator");

            Response.Headers.Add("X-Issuer-Subject", $"{issuer};{subject}");

            return Enumerable.Empty<ResponseModels.Person>();
        }
    }
}