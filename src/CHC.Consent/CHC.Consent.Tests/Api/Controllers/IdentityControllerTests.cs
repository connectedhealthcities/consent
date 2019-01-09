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
using IdentifierValue = CHC.Consent.Api.Client.Models.IdentifierValue;


namespace CHC.Consent.Tests.Api.Controllers
{
    using Random = Testing.Utils.Random;
    using Identifiers = Identifiers.Definitions;

    public static class IdentifierValueExtensions
    {
        public static CHC.Consent.Api.Client.Models.IdentifierValue Value<T>(
            this CHC.Consent.Common.Identity.Identifiers.IdentifierDefinition definition,
            T value)
            => new CHC.Consent.Api.Client.Models.IdentifierValue {Name = definition.SystemName, Value = value};
        
        public static IdentifierValueDto Dto<T>(
            this CHC.Consent.Common.Identity.Identifiers.IdentifierDefinition definition,
            T value)
            => new IdentifierValueDto {Name = definition.SystemName, Value = value};
    }

    public static class ClientIdentifierValues
    {
        public static CHC.Consent.Api.Client.Models.IdentifierValue Address(
            string line1 = null,
            string line2 = null,
            string line3 = null,
            string line4 = null,
            string line5 = null,
            string postcode = null)
        {
            return Identifiers.Address.Value(
                NotNull(
                    Identifiers.AddressLine1.Value(line1),
                    Identifiers.AddressLine2.Value(line2),
                    Identifiers.AddressLine3.Value(line2),
                    Identifiers.AddressLine4.Value(line4),
                    Identifiers.AddressLine5.Value(line5),
                    Identifiers.AddressPostcode.Value(postcode)));
        }

        private static IdentifierValue[] NotNull(params IdentifierValue[] identifierValues)
        {
            return identifierValues.Where(_ => _.Value != null).ToArray();
        }

        public static CHC.Consent.Api.Client.Models.IdentifierValue Name(
            string firstName=null,
            string lastName=null
        )
        {
            return Identifiers.Name.Value(
                NotNull(

                    Identifiers.FirstName.Value(firstName),
                    Identifiers.LastName.Value(lastName)
                )
            );
        }
    }
    
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
                    new CHC.Consent.Api.Client.Models.PersonSpecification
                    {
                        Identifiers =
                        {
                            Identifiers.DateOfBirth.Value(1.December(2018)),
                            Identifiers.HospitalNumber.Value("1112333"),
                            nhsNumber,
                            ClientIdentifierValues.Address("3 Sheaf Street", "Leeds", postcode:"LS10 1HD")
                        },
                        MatchSpecifications =
                        {
                            new CHC.Consent.Api.Client.Models.MatchSpecification {Identifiers = new [] {nhsNumber}}
                        }
                    },
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
            A.CallTo(() => identityRepository.FindPersonBy(A<IEnumerable<PersonIdentifier>>.That.IsSameSequenceAs(nhsNumberIdentifier))).Returns(existingPerson);
            
            
                
            var controller = new IdentityController(
                identityRepository,
                A.Fake<IPersonIdentifierListChecker>(),
                Testing.Utils.Identifiers.Registry, 
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
                            Identifiers = new [] {nhsNumberIdentifier}
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
            
            var nhsNumberIdentifier = Identifiers.NhsNumber.Dto("New NHS Number");
            var bradfordHospitalNumberIdentifier = Identifiers.HospitalNumber.Dto("New HospitalNumber");
            
            var identityRepository = A.Fake<IIdentityRepository>();
            A.CallTo(
                    () => identityRepository.FindPersonBy(
                        A<IEnumerable<PersonIdentifier>>.That.IsSameSequenceAs(nhsNumberIdentifier)))
                .Returns(null);
            
            
            
            var controller = new IdentityController(
                identityRepository,
                A.Fake<IPersonIdentifierListChecker>(),
                Testing.Utils.Identifiers.Registry, 
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
                            Identifiers = new []{nhsNumberIdentifier}
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