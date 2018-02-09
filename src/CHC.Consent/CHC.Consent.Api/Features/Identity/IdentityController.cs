using System;
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
        private PersonSpecificationParser PersonSpecificationParser { get; }
        private IdentityRepository IdentityRepository { get; }

        public IdentityController(IdentityRepository identityRepository)
        {
            IdentityRepository = identityRepository;
            PersonSpecificationParser = new PersonSpecificationParser(identityRepository);
        }

        [Route("/{id:int}")]
        [HttpGet]
        public IActionResult GetPerson(int id)
        {
            return new NotImplementedResult();
        }

        [HttpPut]
        public IActionResult PutPerson([FromBody]PersonSpecification personSpecification)
        {
            var identifiers = PersonSpecificationParser.Parse(personSpecification);

            var person = IdentityRepository.FindPerson(identifiers);

            if (person == null)
            {
                person = IdentityRepository.CreatePerson(identifiers);
                return CreatedAtAction("GetPerson", new {id = person.Id}, null);
            }
            else
            {
                person.UpdatePerson(identifiers);
                return new SeeOtherActionResult("GetPerson", new {id = person.Id});
            }
        }
    }
}