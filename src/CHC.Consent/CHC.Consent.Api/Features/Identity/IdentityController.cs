using System.Collections.Generic;
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
        private readonly IPersonIdentifierListChecker registry;
        private IIdentityRepository IdentityRepository { get; }

        public IdentityController(IIdentityRepository identityRepository, IPersonIdentifierListChecker registry)
        {
            this.registry = registry;
            IdentityRepository = identityRepository;
        }

        [Route("/{id:int}")]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type=typeof(IEnumerable<IIdentifier>))]
        [AutoCommit]
        public IActionResult GetPerson(int id)
        {
            return Ok(IdentityRepository.GetPersonIdentities(id));
        }

        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.Created, Type=typeof(long))]
        [ProducesResponseType((int) HttpStatusCode.Found)]
        [AutoCommit]
        public IActionResult PutPerson([FromBody, Required]PersonSpecification specification)
        {
            if(!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            
            registry.EnsureHasNoInvalidDuplicates(specification.Identifiers);
            
            var person = IdentityRepository.FindPerson(specification.MatchSpecifications.Select(_ => _.Identifiers));

            if (person == null)
            {
                person = IdentityRepository.CreatePerson(specification.Identifiers);
                return CreatedAtAction("GetPerson", new {id = person.Id}, person.Id);
            }
            else
            {
                IdentityRepository.UpdatePerson(person, specification.Identifiers);
                return new SeeOtherActionResult("GetPerson", new {id = person.Id});
            }
        }
    }
}