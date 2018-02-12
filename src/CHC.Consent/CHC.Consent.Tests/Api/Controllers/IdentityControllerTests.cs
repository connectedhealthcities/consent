using System.Linq;
using CHC.Consent.Api.Features.Identity;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CHC.Consent.Tests.Api.Controllers
{
    public class IdentityControllerTests
    {
        [Fact]
        public void UpdatesAnExistingPerson()
        {
            var personWithNhsNumberAndHospitalNumber = new Person {NhsNumber = "444-333-111"}.WithBradfordHosptialNumbers("444333111");

            var controller = new IdentityController(
                Create.AnIdentityRepository.WithPeople(personWithNhsNumberAndHospitalNumber),
                new PersonSpecificationParser(
                    new IdentifierTypeRegistry { IdentifierType.NhsNumber, IdentifierType.BradfordHospitalNumber }
                    ));


            var result = controller.PutPerson(
                new PersonSpecification
                {
                    Identifiers =
                    {
                        new IdentifierSpecification
                        {
                            Type = IdentifierType.NhsNumber.ExternalId,
                            Value = personWithNhsNumberAndHospitalNumber.NhsNumber
                        },
                        new IdentifierSpecification
                        {
                            Type = IdentifierType.BradfordHospitalNumber.ExternalId,
                            Value = "Added HospitalNumber",
                        }
                    },
                    MatchSpecifications =
                    {
                        new MatchSpecification
                        {
                            Identifiers = new[]
                            {
                                new MatchIdentifierSpecification
                                {
                                    IdOrType = IdentifierType.NhsNumber.ExternalId,
                                    MatchBy = MatchBy.Type
                                },
                            }
                        }
                    }
                }

            );

            Assert.Contains("Added HospitalNumber", personWithNhsNumberAndHospitalNumber.BradfordHosptialNumbers);

            Assert.IsType<SeeOtherActionResult>(result);
        }
        
        [Fact]
        public void CreatesAPerson()
        {
            var personWithNhsNumberAndHospitalNumber = new Person {NhsNumber = "444-333-111"}.WithBradfordHosptialNumbers("444333111");

            Create.MockStore<Person> peopleStore = Create.AMockStore<Person>().WithContents(personWithNhsNumberAndHospitalNumber);
            var controller = new IdentityController(
                Create.AnIdentityRepository
                    
                    .WithPeopleStore(peopleStore),
                new PersonSpecificationParser(new IdentifierTypeRegistry { IdentifierType.NhsNumber, IdentifierType.BradfordHospitalNumber }));


            var result = controller.PutPerson(
                new PersonSpecification
                {
                    Identifiers =
                    {
                        new IdentifierSpecification
                        {
                            Type = IdentifierType.NhsNumber.ExternalId,
                            Value = "New NHS Number"
                        },
                        new IdentifierSpecification
                        {
                            Type = IdentifierType.BradfordHospitalNumber.ExternalId,
                            Value = "New HospitalNumber",
                        }
                    },
                    MatchSpecifications =
                    {
                        new MatchSpecification
                        {
                            Identifiers = new[]
                            {
                                new MatchIdentifierSpecification
                                {
                                    IdOrType = IdentifierType.NhsNumber.ExternalId,
                                    MatchBy = MatchBy.Type
                                },
                            }
                        }
                    }
                }

            );

            Assert.IsAssignableFrom<CreatedAtActionResult>(result);

            Assert.Single(peopleStore.Additions);
            var addedPerson = peopleStore.Additions.First();
            Assert.Equal("New NHS Number", addedPerson.NhsNumber);
            Assert.Single(addedPerson.BradfordHosptialNumbers);
            Assert.Equal("New HospitalNumber", addedPerson.BradfordHosptialNumbers.First());
        }
    }
}