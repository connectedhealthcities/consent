using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Features.Identity
{
    [Route("/identities")]
    public class IdentityController : Controller
    {
        private readonly PersonIdentifierRegistry registry;

        private IdentityRepository IdentityRepository { get; }

        public IdentityController(IdentityRepository identityRepository, PersonIdentifierRegistry registry)
        {
            this.registry = registry;
            IdentityRepository = identityRepository;
        }

        [Route("/{id:int}")]
        [HttpGet]
        public IActionResult GetPerson(int id)
        {
            return new NotImplementedResult();
        }

        [HttpPut,ProducesResponseType((int)HttpStatusCode.Created),ProducesResponseType((int)HttpStatusCode.Found)]
        public IActionResult PutPerson([FromBody, Required]PersonSpecification specification)
        {
            if(!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            
            registry.EnsureHasNoInvalidDuplicates(specification.Identifiers);
            
            var person = IdentityRepository.FindPerson(specification.MatchSpecifications.Select(_ => _.Identifiers));

            if (person == null)
            {
                person = IdentityRepository.CreatePerson(specification.Identifiers);
                return CreatedAtAction("GetPerson", new {id = person.Id}, null);
            }
            else
            {
                person.UpdatePerson(specification.Identifiers);
                return new SeeOtherActionResult("GetPerson", new {id = person.Id});
            }
        }
    }
}