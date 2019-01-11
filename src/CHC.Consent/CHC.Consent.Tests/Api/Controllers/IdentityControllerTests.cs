﻿using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Features.Identity;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Testing.Utils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;


namespace CHC.Consent.Tests.Api.Controllers
{
    using Random = Testing.Utils.Random;
    using Identifiers = Identifiers.Definitions;
    using static Testing.Utils.IdentifierValueExtensions;

    public class IdentityControllerTests
    {
        private readonly ITestOutputHelper output;


        /// <inheritdoc />
        public IdentityControllerTests(ITestOutputHelper output)
        {
            this.output = output;
        }


        [Fact]
        public void Test()
        {
            var nhsNumber = Identifiers.NhsNumber.Value("32222");
            output.WriteLine(
                JsonConvert.SerializeObject(
                    new CHC.Consent.Api.Client.Models.PersonSpecification(
                    new CHC.Consent.Api.Client.Models.IIdentifierValueDto[]
                        {
                            Identifiers.DateOfBirth.Value(1.December(2018)),
                            Identifiers.HospitalNumber.Value("1112333"),
                            nhsNumber,
                            ClientIdentifierValues.Address("3 Sheaf Street", "Leeds", postcode:"LS10 1HD")
                        },
                        new []
                        {
                            new CHC.Consent.Api.Client.Models.MatchSpecification {Identifiers = new [] {nhsNumber}}
                        }
                    ),
                    Formatting.Indented
                )
            );
        }
        
        [Fact]
        public void UpdatesAnExistingPerson()
        {
         
            var existingPerson = new PersonIdentity(Random.Long());

            var nhsNumberIdentifier = Identifiers.NhsNumber.Dto("444-333-111");
            var bradfordHospitalNumberIdentifier = Identifiers.HospitalNumber.Dto("Added HospitalNumber");
            
            var identityRepository = A.Fake<IIdentityRepository>();
            A.CallTo(() => identityRepository.FindPersonBy(
                A<IEnumerable<PersonIdentifier>>.That.Matches(
                    _ => _.All(i => i.Definition == Testing.Utils.Identifiers.Definitions.NhsNumber))))
                .Returns(existingPerson);


            var registry = Testing.Utils.Identifiers.Registry;
            var controller = new IdentityController(
                identityRepository,
                registry);


            
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
                            Identifiers = new [] {nhsNumberIdentifier}
                        }
                    }
                }

            );

            A.CallTo(
                    () => identityRepository.UpdatePerson(existingPerson,
                        A<IEnumerable<PersonIdentifier>>.That.IsSameSequenceAs(
                            registry.ConvertToPersonIdentifier(nhsNumberIdentifier), 
                            registry.ConvertToPersonIdentifier(bradfordHospitalNumberIdentifier))))
                .MustHaveHappenedOnceExactly();
            Assert.IsType<SeeOtherOjectActionResult>(result);
        }
        
        [Fact]
        public void CreatesAPerson()
        {
            
            var nhsNumberIdentifier = Identifiers.NhsNumber.Dto("New NHS Number");
            var bradfordHospitalNumberIdentifier = Identifiers.HospitalNumber.Dto("New HospitalNumber");

            var registry = Testing.Utils.Identifiers.Registry;
            
            var identityRepository = A.Fake<IIdentityRepository>();
            A.CallTo(
                    () => identityRepository.FindPersonBy(
                        A<IEnumerable<PersonIdentifier>>.That.IsSameSequenceAs(
                            registry.ConvertToPersonIdentifier(nhsNumberIdentifier))))
                .Returns(null);


            var controller = new IdentityController(
                identityRepository,
                registry);


            
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
                            Identifiers = new []{nhsNumberIdentifier}
                        }
                    }
                }

            );

            Assert.IsAssignableFrom<CreatedAtActionResult>(result);

            A.CallTo(
                    () => identityRepository.CreatePerson(
                        A<IEnumerable<PersonIdentifier>>.That.IsSameSequenceAs(
                            registry.ConvertToPersonIdentifier(nhsNumberIdentifier),
                            registry.ConvertToPersonIdentifier(bradfordHospitalNumberIdentifier))))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void CorrectlyConvertsIdentifiers()
        {
            var identityRepository = A.Fake<IIdentityRepository>();
            A.CallTo(
                    () => identityRepository.GetPersonIdentifiers(34))
                .Returns(new []
                {
                    Testing.Utils.Identifiers.NhsNumber("1234"),
                    Testing.Utils.Identifiers.Name("Peng", "Chips")
                });

            var registry = Testing.Utils.Identifiers.Registry;

            var controller = new IdentityController(
                identityRepository,
                registry);

            var result = controller.GetPerson(34);

            result.Should().BeOfType<OkObjectResult>()
                .Subject.Value.Should().BeAssignableTo<IEnumerable<IIdentifierValueDto>>()
                .Subject
                .Should().BeEquivalentTo(
                    Identifiers.NhsNumber.Dto("1234"),
                    Identifiers.Name.Dto(
                        new[]
                        {
                            Identifiers.FirstName.Dto("Peng"),
                            Identifiers.LastName.Dto("Chips"),
                        }
                    ));
        }
    }
}