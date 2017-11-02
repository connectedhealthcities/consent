using CHC.Consent.WebApi.Abstractions.Bootstrap;
using Microsoft.AspNetCore.Mvc;
using Version = CHC.Consent.WebApi.Infrastructure.Version;

namespace CHC.Consent.WebApi.Features.Bootstrap
{
    [Route("/v{version:apiVersion}/bootstrap"),Version._0_1_Dev]
    public class BootstrapController : Controller
    {
        public IBootstrapper Bootstrapper { get; }

        /// <inheritdoc />
        public BootstrapController(IBootstrapper bootstrapper)
        {
            Bootstrapper = bootstrapper;
        }

        /// <summary>
        /// bootstraps the system with the current authenticated user in an admin/god role
        /// </summary>
        [HttpPost]
        public IActionResult Post()
        {
            Bootstrapper.Bootstrap();

            return Ok();
        }
    }
}