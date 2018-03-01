using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace CHC.Consent.Api.Features.Identity
{
    [Route("/identities")]
    public class IdentityController : Controller
    {
        private readonly IPersonIdentifierListChecker identifierChecker;
        private readonly ITypeRegistry<IIdentifier> registry;
        private readonly ArrayPool<char> arrayPool;
        private IIdentityRepository IdentityRepository { get; }

        public IdentityController(
            IIdentityRepository identityRepository, 
            IPersonIdentifierListChecker identifierChecker,
            ITypeRegistry<IIdentifier> registry, 
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
        [ProducesResponseType((int)HttpStatusCode.OK, Type=typeof(IEnumerable<IIdentifier>))]
        [AutoCommit]
        public IActionResult GetPerson(long id)
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
            
            identifierChecker.EnsureHasNoInvalidDuplicates(specification.Identifiers);
            
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