using CHC.Consent.Api.Features.Consent;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.Testing.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CHC.Consent.Tests.Api.Controllers
{
    public class ConsentMetaControllerTests
    {
        [Fact]
        public void ReturnsDataAboutDefinitions()
        {
            var definitions = new[]
            {
                Evidences.String("TestString"),
                Evidences.Enum("TestEnum", "Yes", "No")
            };
            var controller = new MetaController(new EvidenceDefinitionRegistry(definitions));

            var result = controller.Get();

            result.Should().BeOfType<OkObjectResult>()
                .Which
                .Value.Should().BeEquivalentTo(definitions);

        }
    }
}