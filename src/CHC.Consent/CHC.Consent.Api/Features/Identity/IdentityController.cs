using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace CHC.Consent.Api.Features.Identity
{
    [Route("/identities")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class IdentityController : Controller
    {
        private readonly IdentifierDefinitionRegistry registry;
        private PersonIdentifiersDtosIdentifierDtoMarshaller IdentifierIdentifierDtoMarshaller { get; }
        private IIdentityRepository IdentityRepository { get; }

        public IdentityController(
            IIdentityRepository identityRepository, IdentifierDefinitionRegistry registry)
        {
            IdentityRepository = identityRepository;
            this.registry = registry;
            IdentifierIdentifierDtoMarshaller = new PersonIdentifiersDtosIdentifierDtoMarshaller(this.registry);
        }


        [Route("{id:int}")]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type=typeof(IEnumerable<IIdentifierValueDto>))]
        [AutoCommit]
        public IActionResult GetPerson(long id)
        {
            return Ok(IdentifierIdentifierDtoMarshaller.MarshallToDtos(IdentityRepository.GetPersonIdentifiers(id)));
        }

        [HttpPost("search")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type=typeof(SearchResult))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult FindPerson([FromBody, Required] MatchSpecification[] match)
        {
            if(!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            
            var person = FindMatchingPerson(match);

            return
                person == null
                    ? (IActionResult) NotFound()
                    : Ok(new SearchResult {PersonId = person});
        }

        private PersonIdentity FindMatchingPerson(IEnumerable<MatchSpecification> match)
        {
            return IdentityRepository.FindPerson(match.Select(_ => IdentifierIdentifierDtoMarshaller.ConvertToIdentifiers(_.Identifiers)));
        }

        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.Created, Type=typeof(PersonCreatedResult))]
        [ProducesResponseType((int) HttpStatusCode.SeeOther, Type=typeof(PersonCreatedResult))]
        [AutoCommit]
        public IActionResult PutPerson([FromBody, Required]PersonSpecification specification)
        {
            if(!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            var authority = IdentityRepository.GetAuthority(specification.Authority);
            if(authority == null)
                ModelState.AddModelError(nameof(specification.Authority), $"Authority '{specification.Authority}' does not exist");
            ValidateIdentifierTypes(specification.Identifiers, nameof(specification.Identifiers));
            ValidateIdentifierTypes(
                specification.MatchSpecifications.SelectMany(_ => _.Identifiers),
                nameof(specification.MatchSpecifications));
            if(!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            //identifierChecker.EnsureHasNoInvalidDuplicates(specification.Identifiers);

            var identifiers = IdentifierIdentifierDtoMarshaller.ConvertToIdentifiers(specification.Identifiers);
            

            var person = FindMatchingPerson(specification.MatchSpecifications);

            if (person == null)
            {
                person = IdentityRepository.CreatePerson(identifiers, authority);
                return CreatedAtAction("GetPerson", new {id = person.Id}, new PersonCreatedResult {PersonId = person});
            }
            else
            {
                IdentityRepository.UpdatePerson(person, identifiers, authority);

                return new SeeOtherOjectActionResult(
                    "GetPerson",
                    routeValues: new {id = person.Id},
                    result: new PersonCreatedResult {PersonId = person});
            }
        }

        private void ValidateIdentifierTypes(IEnumerable<IIdentifierValueDto> identifiers, string modelStateName)
        {
            foreach (var identifier in identifiers)
            {
                if (registry.IsValidIdentifierType(identifier)) continue;
                ModelState.AddModelError(
                    modelStateName,
                    $"'{identifier.SystemName}' is not a valid identifier type");
            }
        }
    }
}