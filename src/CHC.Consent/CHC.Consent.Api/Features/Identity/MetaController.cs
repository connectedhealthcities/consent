using System.Collections.Generic;
using System.Linq;
using System.Net;
using CHC.Consent.Common.Identity.Identifiers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Features.Identity
{
    [Route("/identities/meta", Name="Identity Store Metadata")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MetaController : Controller
    {
        private IdentifierDefinitionRegistry IdentifierDefinitionRegistry { get; }

        /// <inheritdoc />
        public MetaController(IdentifierDefinitionRegistry identifierDefinitionRegistry)
        {
            IdentifierDefinitionRegistry = identifierDefinitionRegistry;
        }

        [HttpGet(Name = "GetIdentityStoreMetadata")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type=typeof(IEnumerable<IdentifierDefinition>))]
        public IActionResult Get()
        {
            return Ok(IdentifierDefinitionRegistry.ToArray<IdentifierDefinition>());
        }
    }
}