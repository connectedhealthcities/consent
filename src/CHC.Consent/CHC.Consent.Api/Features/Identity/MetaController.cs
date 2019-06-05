using System.Collections.Generic;
using System.Linq;
using System.Net;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CHC.Consent.Api.Features.Identity
{
    [Route("/identities/meta", Name = "Identity Store Metadata")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MetaController : Controller
    {
        private readonly IIdentityRepository repository;

        /// <inheritdoc />
        public MetaController(IdentifierDefinitionRegistry registry, IIdentityRepository repository)
        {
            this.repository = repository;
            Registry = registry;
        }

        private IdentifierDefinitionRegistry Registry { get; }

        [HttpGet(Name = "GetIdentityStoreMetadata")]
        [Infrastructure.Web.ProducesResponseType(HttpStatusCode.OK, Type = typeof(IEnumerable<IdentifierDefinition>))]
        public IActionResult Get()
        {
            return Ok(Registry.ToArray<IdentifierDefinition>());
        }

        [HttpGet("/agency/{agencyName}/", Name = "GetAgencyIdentifiersAndFieldNamesMetadata")]
        [Infrastructure.Web.ProducesResponseType(typeof(AgencyInfoDto), HttpStatusCode.OK)]
        [Infrastructure.Web.ProducesResponseType(HttpStatusCode.NotFound)]
        public IActionResult GetAgencyInfo([NotNull, FromRoute] string agencyName)
        {
            var agency = repository.GetAgency(agencyName);
            return Ok(
                new AgencyInfoDto
                {
                    Agency = agency,
                    Identifiers = agency.RootIdentifierNames().Select(_ => (IdentifierDefinition) Registry[_]).ToArray()
                });
        }
    }
}