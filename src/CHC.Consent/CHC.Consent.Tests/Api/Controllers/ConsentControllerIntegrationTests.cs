using System.Linq;
using System.Net.Http;
using CHC.Consent.Api.Client;
using CHC.Consent.Api.Client.Models;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Consent;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.Testing.Utils;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.Tests.Api.Controllers
{
    [Collection(WebServerCollection.Name),Trait("Category", "Integration")]
    public class ConsentControllerIntegrationTests
    {
        public ITestOutputHelper Output { get; }
        private HttpClient Client { get; }
        public TestServer Server { get; set; }


        /// <inheritdoc />
        public ConsentControllerIntegrationTests(WebServerFixture fixture, ITestOutputHelper output)
        {
            Output = output;
            fixture.Output = output;
            Client = fixture.Client;
            Server = fixture.Server;
        }

        [Fact()]
        public void SavesConsent()
        {
            var consentContext = Server.Host.Services.GetService<ConsentContext>();
            
            var study = consentContext.Add(new StudyEntity{Name = Random.String()}).Entity;
            var person = consentContext.Add(new PersonEntity()).Entity;
            consentContext.SaveChanges();
            
            var client = new CHC.Consent.Api.Client.Api(Client, disposeHttpClient:false);
            var api = (IApi) client;

            var result = api.ConsentPut(
                new ConsentSpecification
                {
                    StudyId = study.Id,
                    CaseId = new CaseIdentifier[]
                        {new UkNhsBradfordhospitalsBib4allConsentPregnancyNumber("1"),},
                    DateGiven = Random.Date().Date,
                    GivenBy = person.Id,
                    PersonId = person.Id,
                    SubjectIdentifier = Random.String(15),
                    Evidence = new Evidence[]
                    {
                        new UkNhsBradfordhospitalsBib4allEvidenceMedway
                        {
                            CompetentStatus = "Competent"
                        }
                    }
                });
            
            Assert.NotNull(result);
            var newConsentId = Assert.IsType<long>(result);
            var consentEntity = consentContext.Set<ConsentEntity>()
                .Include(_ => _.StudySubject)
                .ThenInclude(_ => _.Study)
                .Include(_ => _.StudySubject)
                .ThenInclude(_ => _.Person)
                .Include(_ => _.GivenBy)
                .Single(_ => _.Id == newConsentId);
            Assert.NotNull(consentEntity);
            Assert.Equal(1, consentContext.Set<GivenEvidenceEntity>().Count(_ => _.Consent.Id == newConsentId));
            Assert.Equal(1, consentContext.Set<CaseIdentifierEntity>().Count(_ => _.Consent.Id == newConsentId));

        }
    }
}