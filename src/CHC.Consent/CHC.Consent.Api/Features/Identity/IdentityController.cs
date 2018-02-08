using System;
using CHC.Consent.Api.Features.Identity.Dto;
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

        [HttpGet]
        public IActionResult GetPerson(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        public IActionResult PutPerson(PersonSpecification personSpecification)
        {
            var identifiers = PersonSpecificationParser.Parse(personSpecification);

            var person = IdentityRepository.FindPerson(identifiers);

            if (person == null)
            {
                person = IdentityRepository.CreatePerson(identifiers);
                return CreatedAtAction("GetPerson", new {id = person.Id});
            }
            else
            {
                person.UpdatePerson(identifiers);
                return RedirectToAction("GetPerson", new {id = person.Id});
            }
        }
    }
}