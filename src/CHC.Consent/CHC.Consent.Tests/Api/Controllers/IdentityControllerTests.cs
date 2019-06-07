using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Api.Features.Identity;
using CHC.Consent.Api.Features.Identity.Dto;
using CHC.Consent.Api.Infrastructure.Web;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.Testing.Utils;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using Agency = CHC.Consent.Common.Identity.Agency;
using AgencyPersonDto = CHC.Consent.Api.Features.Identity.AgencyPersonDto;
using IdentifierDefinition = CHC.Consent.Common.Identity.Identifiers.IdentifierDefinition;
using IIdentifierValueDto = CHC.Consent.Api.Infrastructure.IIdentifierValueDto;
using MatchSpecification = CHC.Consent.Api.Client.Models.MatchSpecification;
using PersonIdentity = CHC.Consent.Common.PersonIdentity;
using PersonSpecification = CHC.Consent.Api.Client.Models.PersonSpecification;

namespace CHC.Consent.Tests.Api.Controllers
{
    using Identifiers = Identifiers.Definitions;
    using static IdentifierValueExtensions;

    public class IdentityControllerTests
    {
        /// <inheritdoc />
        public IdentityControllerTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        private readonly ITestOutputHelper output;


        private IdentityController CreateController(
            IIdentityRepository identityRepository, params IdentifierDefinition[] identifiers)
        {
            var registry = new IdentifierDefinitionRegistry(identifiers);
            return new IdentityController(identityRepository, registry, null);
        }

        private IdentityController CreateController(
            IIdentityRepository identityRepository, IdentifierDefinitionRegistry registry = null)
        {
            return new IdentityController(identityRepository, registry ?? Testing.Utils.Identifiers.Registry, null);
        }


        private PersonIdentifier Identifier(IIdentifierValueDto identifier)
        {
            return new PersonIdentifier(
                new SimpleIdentifierValue(identifier.Value),
                (IdentifierDefinition) Testing.Utils.Identifiers.Registry[identifier.SystemName]);
        }

        private static PersonIdentifier PersonIdentifier<T>(T value, IdentifierDefinition definition)
            => Testing.Utils.Identifiers.PersonIdentifier(value, definition);

        [Fact]
        public void CorrectlyConvertsIdentifiers()
        {
            var identityRepository = A.Fake<IIdentityRepository>();
            A.CallTo(
                    () => identityRepository.GetPersonIdentifiers(34))
                .Returns(
                    new[]
                    {
                        Testing.Utils.Identifiers.NhsNumber("1234"),
                        Testing.Utils.Identifiers.Name("Peng", "Chips")
                    });

            var controller = CreateController(identityRepository);

            var result = controller.GetPerson(34);

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<IEnumerable<IIdentifierValueDto>>()
                .Which
                .Should().BeEquivalentTo(
                    Identifiers.NhsNumber.Dto("1234"),
                    Identifiers.Name.Dto(
                        new[]
                        {
                            Identifiers.FirstName.Dto("Peng"),
                            Identifiers.LastName.Dto("Chips")
                        }
                    ));
        }

        [Fact]
        public void CreatesAPerson()
        {
            var nhsNumberIdentifier = Identifiers.NhsNumber.Dto("New NHS Number");
            var bradfordHospitalNumberIdentifier = Identifiers.HospitalNumber.Dto("New HospitalNumber");

            const string authorityName = "grissly-bear";
            var identityRepository = A.Fake<IIdentityRepository>();
            var authority = new Authority {SystemName = authorityName};
            A.CallTo(
                    () => identityRepository.FindPersonBy(
                        A<CompositePersonSpecification>._)
                )
                .Returns(null);
            A.CallTo(() => identityRepository.GetAuthority(authorityName)).Returns(authority);


            var controller = CreateController(identityRepository);


            var result = controller.PutPerson(
                new CHC.Consent.Api.Features.Identity.Dto.PersonSpecification
                {
                    Authority = authorityName,
                    Identifiers =
                    {
                        nhsNumberIdentifier,
                        bradfordHospitalNumberIdentifier
                    },
                    MatchSpecifications =
                    {
                        new IdentifierMatchSpecification{Identifiers = new[] {nhsNumberIdentifier}}
                    }
                }
            );

            Assert.IsAssignableFrom<CreatedAtActionResult>(result);

            A.CallTo(
                    () =>
                        identityRepository.CreatePerson(
                            A<IEnumerable<PersonIdentifier>>.That.IsSameSequenceAs(
                                Identifier(nhsNumberIdentifier),
                                Identifier(bradfordHospitalNumberIdentifier)),
                            authority))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void GetCorrectPersonIdentifiersForAgency()
        {
            const string field1Name = "f-1";
            const string field2Name = "f-2";
            const string agencyName = "testing";
            var agencyIdentity = (CHC.Consent.Common.Identity.AgencyIdentity) Random.Long();

            var personId = (PersonIdentity) 1625L;
            var field1 = Identifiers.String(field1Name);
            var field2 = Identifiers.String(field2Name);
            var personAgencyId = Random.String();


            var identityRepository = A.Fake<IIdentityRepository>(_ => _.Strict());

            A.CallTo(() => identityRepository.GetAgency(agencyName))
                .Returns(
                    new Agency
                    {
                        Name = "Testing", Id = agencyIdentity, Fields = new[] {field1Name + "::sub-field", field2Name}
                    });

            A.CallTo(() => identityRepository.GetPersonAgencyId(personId, agencyIdentity))
                .Returns(personAgencyId);


            var getPeopleWithIdentifiersCall = A.CallTo(
                () => identityRepository.GetPeopleWithIdentifiers(
                    A<IEnumerable<PersonIdentity>>.That.IsSameSequenceAs(personId),
                    A<IEnumerable<string>>.That.IsSameSequenceAs(field1Name, field2Name),
                    A<IUserProvider>.Ignored)
            );

            getPeopleWithIdentifiersCall
                .Returns(
                    new Dictionary<PersonIdentity, IEnumerable<PersonIdentifier>>
                    {
                        [personId] = new[]
                        {
                            PersonIdentifier("cs", field1),
                            PersonIdentifier("dt", field2)
                        }
                    });


            var controller = CreateController(identityRepository, field1, field2);

            var result = controller.GetPersonForAgency(personId, agencyName);


            getPeopleWithIdentifiersCall.MustHaveHappenedOnceExactly();

            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeAssignableTo<AgencyPersonDto>()
                .Which.Should().BeEquivalentTo(
                    new AgencyPersonDto(personAgencyId, new[] {field1.Dto("cs"), field2.Dto("dt")})
                );
        }

        [Fact]
        public void Test()
        {
            var nhsNumber = Identifiers.NhsNumber.Value("32222");
            output.WriteLine(
                JsonConvert.SerializeObject(
                    new PersonSpecification(
                        new CHC.Consent.Api.Client.Models.IIdentifierValueDto[]
                        {
                            Identifiers.DateOfBirth.Value(1.December(2018)),
                            Identifiers.HospitalNumber.Value("1112333"),
                            nhsNumber,
                            ClientIdentifierValues.Address("3 Sheaf Street", "Leeds", postcode: "LS10 1HD")
                        },
                        "medway",
                        new[]
                        {
                            new CHC.Consent.Api.Client.Models.IdentifierMatchSpecification(
                                new CHC.Consent.Api.Client.Models.IIdentifierValueDto[] {nhsNumber})
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
            A.CallTo(
                    () => identityRepository.FindPersonBy(
                        A<CompositePersonSpecification>.That.Matches(
                            _ => _.Specifications.Cast<PersonIdentifierSpecification>().All(
                                i => i.PersonIdentifier.Definition == Identifiers.NhsNumber))))
                .Returns(existingPerson);

            var authority = new Authority {SystemName = "shabba-ranks"};
            A.CallTo(() => identityRepository.GetAuthority("shabba-ranks"))
                .Returns(authority);


            var controller = CreateController(identityRepository);

            var result = controller.PutPerson(
                new CHC.Consent.Api.Features.Identity.Dto.PersonSpecification
                {
                    Authority = "shabba-ranks",
                    Identifiers =
                    {
                        nhsNumberIdentifier,
                        bradfordHospitalNumberIdentifier
                    },
                    MatchSpecifications =
                    {
                        new IdentifierMatchSpecification{Identifiers = new[] {nhsNumberIdentifier}}
                    }
                }
            );

            A.CallTo(
                    () => identityRepository.UpdatePerson(
                        existingPerson,
                        A<IEnumerable<PersonIdentifier>>.That.IsSameSequenceAs(
                            Identifier(nhsNumberIdentifier),
                            Identifier(bradfordHospitalNumberIdentifier)),
                        authority)
                )
                .MustHaveHappenedOnceExactly();
            Assert.IsType<SeeOtherOjectActionResult>(result);
        }
    }
}