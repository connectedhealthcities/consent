using System;
using System.Linq;
using CHC.Consent.Common.Consent.Evidences;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.Testing.Utils;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.Tests.Api.Controllers
{
    [Collection(WebServerCollection.Name), Trait("Category", "Integration")]
    public class WebIntegrationTest
    {
        /// <inheritdoc />
        public WebIntegrationTest(WebServerFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            Output = output;
            fixture.Output = output;
            Server = fixture.Server;
            ApiClient = fixture.ApiClient;
            AddData(
                c =>
                {
                    c.Database.EnsureDeleted();
                    c.Database.Migrate();
                    return 0;
                });
            AddData(
                ctx =>
                {
                    ctx.AddRange(
                        Identifiers.Registry.Cast<Common.Identity.Identifiers.IdentifierDefinition>()
                            .Select(_ => new IdentifierDefinitionEntity(_.Name, _.ToDefinition()))
                    );
                    ctx.AddRange(
                        KnownEvidence.Registry.Cast<EvidenceDefinition>()
                            .Tap(_ => output.WriteLine("Adding evidence {0}, {1}", _.Name, _.ToDefinition()))
                            .Select(_ => new EvidenceDefinitionEntity(_.Name, _.ToDefinition()))
                        );
                    return 0;
                }
            );
        }

        private WebServerFixture Fixture { get; }
        protected ITestOutputHelper Output { get; }
        
        protected TestServer Server { get; }
        protected Consent.Api.Client.Api ApiClient { get; }

        protected T AddData<T>(Func<ConsentContext, T> add) => Fixture.AddData(add);
    }
}