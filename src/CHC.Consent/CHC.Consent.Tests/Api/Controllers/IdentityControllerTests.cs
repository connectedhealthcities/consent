using System.Linq;
using CHC.Consent.Api.Features.Identity;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CHC.Consent.Tests.Api.Controllers
{
    public class IdentityControllerTests
    {
        private readonly IdentifierRegistry identifierRegistry =
            Create.IdentifierRegistry.WithIdentifiers<NhsNumberIdentifier, BradfordHospitalNumberIdentifier>();

        [Fact]
        public void UpdatesAnExistingPerson()
        {
            const string hospitalNumber = "444333111";
            const string nhsNumber = "444-333-111";
            var personWithNhsNumberAndHospitalNumber = new Person {NhsNumber = nhsNumber }.WithBradfordHosptialNumbers(hospitalNumber);

            var controller = new IdentityController(
                Create.AnIdentityRepository.WithPeople(personWithNhsNumberAndHospitalNumber),
                identifierRegistry);


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

            Create.MockStore<Person> peopleStore =
                Create.AMockStore<Person>().WithContents(personWithNhsNumberAndHospitalNumber);
            var controller = new IdentityController(
                Create.AnIdentityRepository.WithPeopleStore(peopleStore),
                identifierRegistry);


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
    }

}