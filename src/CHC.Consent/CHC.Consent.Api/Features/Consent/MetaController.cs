using System.Collections.Generic;
using System.Linq;
using System.Net;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Common.Identity.Identifiers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Features.Consent
{
    [Route("/consent/meta", Name="Consent Store Metadata")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MetaController : Controller
    {
        private EvidenceDefinitionRegistry Registry { get; }

        /// <inheritdoc />
        public MetaController(EvidenceDefinitionRegistry registry)
        {
            Registry = registry;
        }

        [HttpGet(Name="GetConsentStoreMetadata")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type=typeof(IEnumerable<EvidenceDefinition>))]
        public IActionResult Get()
        {
            return Ok(Registry.ToArray());
        }
    }
}