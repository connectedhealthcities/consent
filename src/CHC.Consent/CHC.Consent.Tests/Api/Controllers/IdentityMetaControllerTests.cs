using CHC.Consent.Api.Features.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Testing.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CHC.Consent.Tests.Api.Controllers
{
    public class IdentityMetaControllerTests
    {
        [Fact]
        public void ReturnsDataAboutDefinitions()
        {
            var definitions = new[]
            {
                Identifiers.Definitions.String("TestString"),
                Identifiers.Definitions.Enum("TestEnum", "Yes", "No")
            };
            var controller = new MetaController(new IdentifierDefinitionRegistry(definitions));

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>()
                .Which
                .Value.Should().BeEquivalentTo(definitions);

        }
    }
}