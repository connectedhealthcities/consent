using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using CHC.Consent.Api;
using CHC.Consent.Api.Features.Identity;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.Testing.Utils;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Rest;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.Tests.Api.Controllers
{
    [Collection(WebServerCollection.Name)]
    public class IdentityControllerTests
    {
        private readonly WebServerFixture serverFixture;
        private readonly ITestOutputHelper output;

        private static readonly string PersonSpecificationJson =
            new StreamReader(
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(
                        typeof(IdentityControllerTests),
                        "PersonSpecification.json"),
                    Encoding.UTF8)
                .ReadToEnd();


        private readonly ITypeRegistry<IPersonIdentifier> personIdentifierRegistry =
            Create.IdentifierRegistry.WithIdentifiers<NhsNumberIdentifier, BradfordHospitalNumberIdentifier>().Build();

        


        /// <inheritdoc />
        public IdentityControllerTests(WebServerFixture serverFixture, ITestOutputHelper output)
        {
            this.serverFixture = serverFixture;
            this.output = output;
        }

        [Fact]
        public void Test()
        {
            var nhsNumber = Create.NhsNumber("32222");
            output.WriteLine(
                JsonConvert.SerializeObject(
                    new PersonSpecification
                    {
                        Identifiers =
                        {
                            Create.DateOfBirth(2018, 12, 1),
                            Create.BradfordHospitalNumber("1112333"),
                            nhsNumber
                        },
                        MatchSpecifications =
                        {
                            new MatchSpecification {Identifiers = new IPersonIdentifier[] {nhsNumber}}
                        }
                    },
                    Formatting.Indented,
                    Startup.SerializerSettings(
                        new JsonSerializerSettings())
                )
            );
        }
        
        [Fact]
        public void UpdatesAnExistingPerson()
        {
            var existingPerson = new PersonIdentity(Random.Long());

            var nhsNumberIdentifier = new NhsNumberIdentifier("444-333-111");
            var bradfordHospitalNumberIdentifier = new BradfordHospitalNumberIdentifier("Added HospitalNumber");
            
            var identityRepository = A.Fake<IIdentityRepository>();
            A.CallTo(() => identityRepository.FindPersonBy(A<IEnumerable<IPersonIdentifier>>.That.IsSameSequenceAs(nhsNumberIdentifier))).Returns(existingPerson);
            
            
                
            var controller = new IdentityController(
                identityRepository,
                A.Fake<IPersonIdentifierListChecker>(),
                personIdentifierRegistry,
                ArrayPool<char>.Create());


            
            var result = controller.PutPerson(
                new PersonSpecification
                {
                    Identifiers =
                    {
                        nhsNumberIdentifier,
                        bradfordHospitalNumberIdentifier,
                    },
                    MatchSpecifications =
                    {
                        new MatchSpecification
                        {
                            Identifiers = new IPersonIdentifier[] {nhsNumberIdentifier}
                        }
                    }
                }

            );

            A.CallTo(
                    () => identityRepository.UpdatePerson(existingPerson,
                        A<IEnumerable<IPersonIdentifier>>.That.IsSameSequenceAs(nhsNumberIdentifier, bradfordHospitalNumberIdentifier)))
                .MustHaveHappenedOnceExactly();
            Assert.IsType<SeeOtherOjectActionResult>(result);
        }
        
        [Fact]
        public void CreatesAPerson()
        {
            var nhsNumberIdentifier = new NhsNumberIdentifier("New NHS Number");
            var bradfordHospitalNumberIdentifier = new BradfordHospitalNumberIdentifier("New HospitalNumber");
            
            var identityRepository = A.Fake<IIdentityRepository>();
            A.CallTo(
                    () => identityRepository.FindPersonBy(
                        A<IEnumerable<IPersonIdentifier>>.That.IsSameSequenceAs(nhsNumberIdentifier)))
                .Returns(null);
            
            
            
            var controller = new IdentityController(
                identityRepository,
                A.Fake<IPersonIdentifierListChecker>(),
                personIdentifierRegistry,
                ArrayPool<char>.Create());


            
            var result = controller.PutPerson(
                new PersonSpecification
                {
                    Identifiers =
                    {
                        
                        nhsNumberIdentifier,
                        bradfordHospitalNumberIdentifier
                    },
                    MatchSpecifications =
                    {
                        new MatchSpecification
                        {
                            Identifiers = new IPersonIdentifier[]{nhsNumberIdentifier}
                        }
                    }
                }

            );

            Assert.IsAssignableFrom<CreatedAtActionResult>(result);

            A.CallTo(
                    () => identityRepository.CreatePerson(
                        A<IEnumerable<IPersonIdentifier>>.That.IsSameSequenceAs(
                            nhsNumberIdentifier,
                            bradfordHospitalNumberIdentifier)))
                .MustHaveHappenedOnceExactly();
        }


        [Fact]
        public void CanDeserializePersonSpecificationFromJson()
        {
            var personSpecification =
                JsonConvert.DeserializeObject<PersonSpecification>(
                    PersonSpecificationJson,
                    Create.IdentifierRegistry
                        .WithIdentifier<NhsNumberIdentifier>()
                        .WithIdentifier<DateOfBirthIdentifier>()
                        .WithIdentifier<BradfordHospitalNumberIdentifier>()
                        .Build()
                        .CreateSerializerSettings());
            
            Assert.NotNull(personSpecification);
            var identifier = Assert.Single(personSpecification.Identifiers);
            var nhsNumber = Assert.IsType<NhsNumberIdentifier>(identifier);
            Assert.Equal("111-222-333", nhsNumber.Value);

            var matchSpecifications = personSpecification.MatchSpecifications;
            var matchSpecification =  Assert.Single(matchSpecifications);
            var matchIdentifier = Assert.Single(matchSpecification.Identifiers);
            Assert.NotNull(matchIdentifier);
            var matchNhsNumber = Assert.IsType<NhsNumberIdentifier>(matchIdentifier);
            Assert.Equal(nhsNumber.Value, matchNhsNumber.Value);

        }


        [Fact]
        public async void HandlesPutPerson()
        {
            var client = serverFixture.Client;
            
            var stringContent = new StringContent(
                PersonSpecificationJson,
                Encoding.UTF8,
                "application/json");

            var response = await client.PutAsync(
                "/identities",
                stringContent);

            if (!Equals(HttpStatusCode.Created, response.StatusCode))
            {
                output.WriteLine(response.AsFormattedString());
            }
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }
}