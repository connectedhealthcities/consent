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
        private IIdentityRepository IdentityRepository { get; }
        private IDictionary<string, PersonIdentifierIdentifierValueDtoMarshallerCreator.IMarshaller> DtoMarshallers { get;  }

        public IdentityController(
            IIdentityRepository identityRepository,
            IdentifierDefinitionRegistry registry)
        {
        
            this.registry = registry;
            IdentityRepository = identityRepository;
            var marshallerCreator = new PersonIdentifierIdentifierValueDtoMarshallerCreator();
            registry.Accept(marshallerCreator);
            DtoMarshallers = marshallerCreator.Marshallers;
        }


        [Route("{id:int}")]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type=typeof(IEnumerable<IIdentifierValueDto>))]
        [AutoCommit]
        public IActionResult GetPerson(long id)
        {
            var identifierValueDtos = IdentityRepository.GetPersonIdentifiers(id)
                .Select(identifier => DtoMarshallers[identifier.Definition.SystemName].MarshallToDto(identifier))
                .ToArray();
            
            return Ok(identifierValueDtos);
        }

        [HttpPost("search")]
        [Produces(typeof(SearchResult))]
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
            return IdentityRepository.FindPerson(match.Select(_ => ConvertToIdentifiers(_.Identifiers)));
        }

        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.Created, Type=typeof(PersonCreatedResult))]
        [ProducesResponseType((int) HttpStatusCode.SeeOther, Type=typeof(PersonCreatedResult))]
        [AutoCommit]
        public IActionResult PutPerson([FromBody, Required]PersonSpecification specification)
        {
            if(!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            ValidateIdentifiers(specification.Identifiers, nameof(specification.Identifiers));
            ValidateIdentifiers(
                specification.MatchSpecifications.SelectMany(_ => _.Identifiers),
                nameof(specification.MatchSpecifications));
            if(!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            //identifierChecker.EnsureHasNoInvalidDuplicates(specification.Identifiers);

            var identifiers = ConvertToIdentifiers(specification.Identifiers);
            

            var person = FindMatchingPerson(specification.MatchSpecifications);

            if (person == null)
            {
                person = IdentityRepository.CreatePerson(identifiers);
                return CreatedAtAction("GetPerson", new {id = person.Id}, new PersonCreatedResult {PersonId = person});
            }
            else
            {
                IdentityRepository.UpdatePerson(person, identifiers);

                return new SeeOtherOjectActionResult(
                    "GetPerson",
                    routeValues: new {id = person.Id},
                    result: new PersonCreatedResult {PersonId = person});
            }
        }

        private PersonIdentifier[] ConvertToIdentifiers(IEnumerable<IIdentifierValueDto> identifiers)
        {
            return identifiers
                .Select(identifier => DtoMarshallers[identifier.DefinitionSystemName].MarshallToIdentifier(identifier))
                .ToArray();
        }

        private void ValidateIdentifiers(IEnumerable<IIdentifierValueDto> identifiers, string modelStateName)
        {
            foreach (var identifier in identifiers)
            {
                if (registry.IsValid(identifier)) continue;
                ModelState.AddModelError(
                    modelStateName,
                    $"'{identifier.DefinitionSystemName}' is not a valid identifier type");
            }
        }
    }
}