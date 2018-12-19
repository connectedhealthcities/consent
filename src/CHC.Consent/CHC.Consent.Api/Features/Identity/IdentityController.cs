using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
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
        private readonly IPersonIdentifierListChecker identifierChecker;
        private readonly IdentifierDefinitionRegistry registry;
        private readonly ArrayPool<char> arrayPool;
        private IIdentityRepository IdentityRepository { get; }

        public IdentityController(
            IIdentityRepository identityRepository, 
            IPersonIdentifierListChecker identifierChecker,
            IdentifierDefinitionRegistry registry, 
            ArrayPool<char> arrayPool)
        {
            this.identifierChecker = identifierChecker;
            this.registry = registry;
            this.arrayPool = arrayPool;
            IdentityRepository = identityRepository;
        }

        /// <inheritdoc />
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objectResult)
            {
                objectResult.Formatters.Add(new JsonOutputFormatter(registry.CreateSerializerSettings(), arrayPool));
            }
            base.OnActionExecuted(context);
        }

        [Route("{id:int}")]
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type=typeof(IEnumerable<IPersonIdentifier>))]
        [AutoCommit]
        public IActionResult GetPerson(long id)
        {
            return Ok(IdentityRepository.GetPersonIdentifiers(id));
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
            return IdentityRepository.FindPerson(match.Select(_ => _.Identifiers.Cast<PersonIdentifier>()));
        }

        [HttpPut]
        [ProducesResponseType((int) HttpStatusCode.Created, Type=typeof(PersonCreatedResult))]
        [ProducesResponseType((int) HttpStatusCode.SeeOther, Type=typeof(PersonCreatedResult))]
        [AutoCommit]
        public IActionResult PutPerson([FromBody, Required]PersonSpecification specification)
        {
            if(!ModelState.IsValid) return new BadRequestObjectResult(ModelState);
            
            identifierChecker.EnsureHasNoInvalidDuplicates(specification.Identifiers);
            
            var person = FindMatchingPerson(specification.MatchSpecifications);

            if (person == null)
            {
                person = IdentityRepository.CreatePerson(specification.Identifiers.Cast<PersonIdentifier>());
                return CreatedAtAction("GetPerson", new {id = person.Id}, new PersonCreatedResult {PersonId = person});
            }
            else
            {
                IdentityRepository.UpdatePerson(person, specification.Identifiers.Cast<PersonIdentifier>());

                return new SeeOtherOjectActionResult(
                    "GetPerson",
                    routeValues: new {id = person.Id},
                    result: new PersonCreatedResult {PersonId = person});
            }
        }
    }
}