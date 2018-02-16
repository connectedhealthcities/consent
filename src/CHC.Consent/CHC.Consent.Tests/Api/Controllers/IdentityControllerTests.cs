﻿using System.IO;
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
            

        private readonly PersonIdentifierRegistry personIdentifierRegistry =
            Create.IdentifierRegistry.WithIdentifiers<NhsNumberIdentifier, BradfordHospitalNumberIdentifier>();


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
                            new MatchSpecification {Identifiers = new IIdentifier[] {nhsNumber}}
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
            const string hospitalNumber = "444333111";
            const string nhsNumber = "444-333-111";
            var personWithNhsNumberAndHospitalNumber = new Person {NhsNumber = nhsNumber }.WithBradfordHosptialNumbers(hospitalNumber);

            var controller = new IdentityController(
                Create.AnIdentityRepository.WithPeople(personWithNhsNumberAndHospitalNumber),
                personIdentifierRegistry);


            var nhsNumberIdentifier = new NhsNumberIdentifier(nhsNumber);
            var result = controller.PutPerson(
                new PersonSpecification
                {
                    Identifiers =
                    {
                        nhsNumberIdentifier,
                        new BradfordHospitalNumberIdentifier("Added HospitalNumber"),
                    },
                    MatchSpecifications =
                    {
                        new MatchSpecification
                        {
                            Identifiers = new IIdentifier[] {nhsNumberIdentifier}
                        }
                    }
                }

            );

            Assert.Contains("Added HospitalNumber", personWithNhsNumberAndHospitalNumber.BradfordHospitalNumbers);

            Assert.IsType<SeeOtherActionResult>(result);
        }
        
        [Fact]
        public void CreatesAPerson()
        {
            const string hospitalNumber = "444333111";
            const string nhsNumber = "444-333-111";
            var personWithNhsNumberAndHospitalNumber = 
                new Person {NhsNumber = nhsNumber }.WithBradfordHosptialNumbers(hospitalNumber);

            MockStore<Person> peopleStore =
                Create.AMockStore<Person>().WithContents(personWithNhsNumberAndHospitalNumber);
            var controller = new IdentityController(
                Create.AnIdentityRepository.WithPeopleStore(peopleStore),
                personIdentifierRegistry);


            var newNhsNumberIdentifier = new NhsNumberIdentifier("New NHS Number"); 
            var result = controller.PutPerson(
                new PersonSpecification
                {
                    Identifiers =
                    {
                        
                        newNhsNumberIdentifier,
                        new BradfordHospitalNumberIdentifier("New HospitalNumber")
                    },
                    MatchSpecifications =
                    {
                        new MatchSpecification
                        {
                            Identifiers = new IIdentifier[]{newNhsNumberIdentifier}
                        }
                    }
                }

            );

            Assert.IsAssignableFrom<CreatedAtActionResult>(result);

            Assert.Single(peopleStore.Additions);
            var addedPerson = peopleStore.Additions.First();
            Assert.Equal("New NHS Number", addedPerson.NhsNumber);
            Assert.Single(addedPerson.BradfordHospitalNumbers);
            Assert.Equal("New HospitalNumber", addedPerson.BradfordHospitalNumbers.First());
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