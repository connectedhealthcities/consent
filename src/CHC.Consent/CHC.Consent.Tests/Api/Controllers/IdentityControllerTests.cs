using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api;
using CHC.Consent.Api.Features.Identity;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Testing.Utils;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.Tests.Api.Controllers
{
    using Random = CHC.Consent.Testing.Utils.Random;
    
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
            var nhsNumber = Identifiers.NhsNumber("32222");
            output.WriteLine(
                JsonConvert.SerializeObject(
                    new PersonSpecification
                    {
                        Identifiers =
                        {
                            Identifiers.DateOfBirth(2018, 12, 1),
                            Identifiers.HospitalNumber("1112333"),
                            nhsNumber,
                            Identifiers.Address("3 Sheaf Street", "Leeds", postcode:"LS10 1HD")
                        },
                        MatchSpecifications =
                        {
                            new MatchSpecification {Identifiers = new IPersonIdentifier[] {nhsNumber}}
                        }
                    },
                    Formatting.Indented,
                    Startup.SerializerSettings(new JsonSerializerSettings(), Identifiers.Registry)
                )
            );
        }
        
        [Fact]
        public void UpdatesAnExistingPerson()
        {
         
            var existingPerson = new PersonIdentity(Random.Long());

            var nhsNumberIdentifier = Identifiers.NhsNumber("444-333-111");
            var bradfordHospitalNumberIdentifier = Identifiers.HospitalNumber("Added HospitalNumber");
            
            var identityRepository = A.Fake<IIdentityRepository>();
            A.CallTo(() => identityRepository.FindPersonBy(A<IEnumerable<PersonIdentifier>>.That.IsSameSequenceAs(nhsNumberIdentifier))).Returns(existingPerson);
            
            
                
            var controller = new IdentityController(
                identityRepository,
                A.Fake<IPersonIdentifierListChecker>(),
                Identifiers.Registry, 
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
                        A<IEnumerable<PersonIdentifier>>.That.IsSameSequenceAs(nhsNumberIdentifier, bradfordHospitalNumberIdentifier)))
                .MustHaveHappenedOnceExactly();
            Assert.IsType<SeeOtherOjectActionResult>(result);
        }
        
        [Fact]
        public void CreatesAPerson()
        {
            
            var nhsNumberIdentifier = Identifiers.NhsNumber("New NHS Number");
            var bradfordHospitalNumberIdentifier = Identifiers.HospitalNumber("New HospitalNumber");
            
            var identityRepository = A.Fake<IIdentityRepository>();
            A.CallTo(
                    () => identityRepository.FindPersonBy(
                        A<IEnumerable<PersonIdentifier>>.That.IsSameSequenceAs(nhsNumberIdentifier)))
                .Returns(null);
            
            
            
            var controller = new IdentityController(
                identityRepository,
                A.Fake<IPersonIdentifierListChecker>(),
                Identifiers.Registry, 
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
                        A<IEnumerable<PersonIdentifier>>.That.IsSameSequenceAs(
                            nhsNumberIdentifier,
                            bradfordHospitalNumberIdentifier)))
                .MustHaveHappenedOnceExactly();
        }


        [Fact(Skip = "Work in progress")]
        public void CanDeserializePersonSpecificationFromJson()
        {
            throw new NotImplementedException("Design in progress");
            /*var personSpecification =
                JsonConvert.DeserializeObject<PersonSpecification>(
                    Data.PersonSpecificationJson,
                    Startup.SerializerSettings(
                        new JsonSerializerSettings(), 
                        Registry
                        )
                    /*
                        .CreateSerializerSettings()#1#);
            
            Assert.NotNull(personSpecification);
            var identifier = Assert.IsType<PersonIdentifier>(personSpecification.Identifiers[0]);
            Assert.Equal("nhs-number", identifier.Definition.SystemName);
            Assert.Equal("111-222-333", identifier.Value.Value);*/

            /*var matchSpecifications = personSpecification.MatchSpecifications;
            var matchSpecification =  Assert.Single(matchSpecifications);
            var matchIdentifier = Assert.Single(matchSpecification.Identifiers);
            Assert.NotNull(matchIdentifier);
            var matchNhsNumber = Assert.IsType<NhsNumberIdentifier>(matchIdentifier);
            Assert.Equal(nhsNumber.Value, matchNhsNumber.Value);*/

        }


        
    }
}