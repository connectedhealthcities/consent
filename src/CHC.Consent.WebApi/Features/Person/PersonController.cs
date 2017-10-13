using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Utils;
using CHC.Consent.WebApi.Abstractions;
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
        private readonly IPersonRepository people;

        /// <inheritdoc />
        public PersonController(IPersonRepository people)
        {
            this.people = people;
        }
        
        [HttpGet]
        public IEnumerable<ResponseModels.Person> Get([FromQuery]RequestModels.GetPeople request)
        {
            return people.GetPeople()
                .GetPage(request.Page, request.PageSize)
                .Select(_ => new ResponseModels.Person {Id = _.Id})
                .AsEnumerable();
        }
    }
}