using System.Linq;
using System.Xml.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.EFCore.Security;
using CHC.Consent.Testing.Utils;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Formatting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.EFCore.Tests
{
    public class IdentityRepositoryTests : DbTests
    {
        /// <inheritdoc />
        public IdentityRepositoryTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(
            outputHelper,
            fixture)
        {
            Formatter.AddFormatter(PersonIdentifierFormatter.Instance);
            Formatter.AddFormatter(DictionaryIdentifierFormatter.Instance);

            personOne = new PersonEntity();
            personTwo = new PersonEntity();
            defaultAuthority = new AuthorityEntity("Default " + Random.String(), 1000, Random.String());

            Context.AddRange(personOne, personTwo);
            Context.Add(defaultAuthority);

            testIdentifierDefinition = Identifiers.Definitions.String("Test");
            Context.AddRange(
                NhsNumberEntity(personOne, personOneNhsNumber),
                NhsNumberEntity(personTwo, personTwoNhsNumber),
                IdentifierEntity(personOne, testIdentifierDefinition, personTwoNhsNumber, defaultAuthority)
            );
            Context.SaveChanges();

            Assert.NotNull(readContext.People.Find(personOne.Id));
            Assert.NotNull(readContext.People.Find(personTwo.Id));
            Assert.Equal(2, readContext.Set<PersonIdentifierEntity>().Count(_ => _.Person == personOne));
            Assert.Single(readContext.Set<PersonIdentifierEntity>().Where(_ => _.Person == personTwo));

            repository = CreateRepository(CreateNewContextInSameTransaction());
        }

        private readonly string personOneNhsNumber = Random.String();
        private readonly string personTwoNhsNumber = Random.String();
        private readonly PersonEntity personOne;
        private readonly PersonEntity personTwo;
        private readonly IdentityRepository repository;
        private readonly IdentifierDefinition testIdentifierDefinition;
        private readonly AuthorityEntity defaultAuthority;

        private PersonIdentity FindPersonByNhsNumber(string nhsNumber)
        {
            return repository.FindPersonBy(Identifiers.NhsNumber(nhsNumber));
        }

        private PersonIdentifierEntity NhsNumberEntity(
            PersonEntity personEntity, string nhsNumber, AuthorityEntity authority = null)
        {
            return IdentifierEntity(personEntity, Identifiers.Definitions.NhsNumber, nhsNumber, authority);
        }

        private PersonIdentifierEntity IdentifierEntity(
            PersonEntity personEntity, IdentifierDefinition identifierDefinition, string value,
            AuthorityEntity authority)
        {
            return new PersonIdentifierEntity
            {
                Person = personEntity,
                TypeName = identifierDefinition.SystemName,
                Value = MarshallValue(identifierDefinition, value),
                ValueType = identifierDefinition.Type.SystemName,
                Authority = authority ?? defaultAuthority
            };
        }

        private static string MarshallValue(IdentifierDefinition identifierDefinition, string value)
        {
            return new IdentifierXmlElementMarshaller<PersonIdentifier, IdentifierDefinition>(identifierDefinition)
                .MarshallToXml(Identifiers.PersonIdentifier(value, identifierDefinition))
                .ToString(SaveOptions.DisableFormatting);
        }

        private IdentityRepository CreateRepository(ConsentContext context)
        {
            var registry = new IdentifierDefinitionRegistry(Identifiers.Definitions.KnownIdentifiers)
            {
                testIdentifierDefinition
            };

            return new IdentityRepository(registry, context);
        }

        private IIncludableQueryable<PersonEntity, AccessControlList> People(ConsentContext context)
        {
            return context.People.AsNoTracking()
                .Include(_ => _.Identifiers)
                .Include(_ => _.ACL);
        }

        [Fact]
        public void AuthorityPriorityIsRespected()
        {
            var higherAuthority =
                createContext.Add(new AuthorityEntity("Higher", defaultAuthority.Priority - 1, "higher")).Entity
                    .ToAuthority();

            var thePerson = CreateRepository(createContext).CreatePerson(
                new[]
                {
                    Identifiers.NhsNumber("1234567890"),
                    Identifiers.Address("22 Medway Street", postcode: "Will Be Replaced")
                },
                defaultAuthority.ToAuthority());
            createContext.SaveChanges();

            var updatedAddress = Identifiers.Address("21 Spine Road", postcode: "Has Replaced");
            CreateRepository(updateContext).UpdatePerson(
                (PersonIdentity) thePerson.Id,
                new[] {updatedAddress},
                higherAuthority);
            updateContext.SaveChanges();

            var newContext = CreateNewContextInSameTransaction();
            CreateRepository(newContext).UpdatePerson(
                (PersonIdentity) thePerson.Id,
                new[] {Identifiers.Address(postcode: "Should Ignore This")},
                defaultAuthority.ToAuthority()
            );
            newContext.SaveChanges();

            var identifiers = readContext.PersonIdentifiers.Where(_ => _.Person == thePerson && _.Deleted == null)
                .ToArray();

            identifiers.Should().ContainSingleIdentifierValue(Identifiers.Definitions.NhsNumber, "1234567890")
                .And
                .ContainSingleIdentifierValue(updatedAddress);
        }

        [Fact]
        public void BringsBackCorrectIdentifiers()
        {
            var personIdentity = new PersonIdentity(personOne.Id);
            var foundDetails = CreateRepository(readContext)
                .GetPeopleWithIdentifiers(
                    new[] {personIdentity},
                    new[] {testIdentifierDefinition.SystemName},
                    null
                );

            foundDetails.Should().ContainKey(personIdentity)
                .WhichValue.Should()
                .OnlyContain(identifier => identifier.Definition == testIdentifierDefinition);
        }

        [Fact]
        public void BringsBackNoIdentifiersWhenPersonHasNoIdentifiers()
        {
            var personIdentity = new PersonIdentity(personTwo.Id);

            var foundDetails = CreateRepository(readContext)
                .GetPeopleWithIdentifiers(
                    new[] {personIdentity},
                    new[] {testIdentifierDefinition.SystemName},
                    null
                );

            foundDetails.Should().ContainKey(personIdentity)
                .WhichValue.Should().BeEmpty();
        }

        [Fact]
        public void CanFindAPersonOneByNhsNumber()
        {
            Assert.Equal(personOne, FindPersonByNhsNumber(personOneNhsNumber));
        }

        [Fact]
        public void CanFindAPersonTwoByNhsNumber() =>
            Assert.Equal(personTwo, FindPersonByNhsNumber(personTwoNhsNumber));

        [Fact]
        public void CanGetAgencies()
        {
            var aField = createContext.Add(new IdentifierDefinitionEntity("A Field", "a-field:string")).Entity;
            createContext.Add(
                new AgencyEntity("External Agency", "external")
                {
                    Fields = {new AgencyFieldEntity {Identifier = aField}}
                }
            );

            createContext.SaveChanges();

            var agency = repository.GetAgency("external");

            using (new AssertionScope())
            {
                agency.Name.Should().Be("External Agency");
                agency.SystemName.Should().Be("external");
                agency.Fields.Should().Contain(new[] {"a-field"});
            }
        }

        [Fact]
        public void CanPersistCompositeIdentifiers()
        {
            var name = Identifiers.Name("Francis", "Drake");
            var nhsNumber = Identifiers.NhsNumber(Random.String());
            var person = CreateRepository(createContext).CreatePerson(
                new[] {name, nhsNumber},
                defaultAuthority.ToAuthority());
            createContext.SaveChanges();

            var personIdentifiers = CreateRepository(readContext).GetPersonIdentifiers(person);

            personIdentifiers.Should().Contain(name)
                .And.Contain(nhsNumber)
                .And.HaveCount(2)
                .And.OnlyHaveUniqueItems();
        }

        [Fact]
        public void CanSearchForMultipleCriteria()
        {
            var found =
                People(readContext).Search(
                        readContext,
                        new HasIdentifiersCriteria(
                            new IdentifierSearch
                                {IdentifierName = testIdentifierDefinition.SystemName, Value = personTwoNhsNumber},
                            new IdentifierSearch
                            {
                                IdentifierName = Identifiers.Definitions.NhsNumber.SystemName,
                                Value = personOneNhsNumber
                            }
                        )
                    )
                    .ToArray();

            found.Should().BeEquivalentTo(personOne);
        }


        [Fact]
        public void CanSearchForSimpleIdentifiers()
        {
            var nhsNumber = Random.String();
            var nhsNumberIdentifier = Identifiers.NhsNumber(nhsNumber);

            var createRepository = CreateRepository(createContext);
            var person = createRepository.CreatePerson(new[] {nhsNumberIdentifier}, defaultAuthority.ToAuthority());
            createContext.SaveChanges();


            var found = People(readContext)
                    .Search(
                        readContext,
                        new HasIdentifiersCriteria(
                            new IdentifierSearch
                                {IdentifierName = nhsNumberIdentifier.Definition.SystemName, Value = nhsNumber}))
                    .ToArray()
                ;

            found.Should().OnlyContain(_ => _.Id == person.Id);
        }

        [Fact]
        public void CorrectlyLoadsCurrentIdentifiers()
        {
            var localRepository = CreateRepository(updateContext);
            var newTestValue = Random.String();
            localRepository.UpdatePerson(
                new PersonIdentity(personOne.Id),
                new[]
                {
                    Identifiers.PersonIdentifier(newTestValue, testIdentifierDefinition)
                },
                defaultAuthority.ToAuthority());
            updateContext.SaveChanges();

            var personIdentifiers = CreateRepository(readContext).GetPersonIdentifiers(personOne.Id).ToArray();

            personIdentifiers.Should().HaveCount(2, "because there are two active identifiers");


            personIdentifiers.Should().ContainSingle(
                _ => Equals(_.Value.Value, personOneNhsNumber) && _.Definition == Identifiers.Definitions.NhsNumber);


            personIdentifiers.Should().ContainSingle(
                _ => Equals(_.Value.Value, newTestValue) && _.Definition == testIdentifierDefinition);
        }

        [Fact]
        public void CreatesAnAgencySpecificIdWhenNoneExists()
        {
            var agencyEntity = new AgencyEntity("test", "test");
            updateContext.Add(agencyEntity);
            updateContext.SaveChanges();


            repository.GetPersonAgencyId((PersonIdentity) personOne.Id, (AgencyIdentity) agencyEntity.Id).Should()
                .NotBeNull();
        }

        [Fact]
        public void CreatesCorrectIdentifiers()
        {
            var testValue = Random.String();
            var nhsNumber = Random.String();
            var newPersonIdentity = CreateRepository(createContext).CreatePerson(
                new[]
                {
                    Identifiers.NhsNumber(nhsNumber),
                    Identifiers.PersonIdentifier(testValue, testIdentifierDefinition)
                },
                defaultAuthority.ToAuthority()
            );
            createContext.SaveChanges();

            var newPerson = readContext.People.Find(newPersonIdentity.Id);
            newPerson.Should().NotBeNull();

            var createdIdentifiers = readContext.PersonIdentifiers.Where(_ => _.Person == newPerson).ToArray();

            createdIdentifiers.Should().HaveCount(2);

            createdIdentifiers.Should().NotContain(_ => _.Deleted != null);
            createdIdentifiers.Should()
                .ContainSingleIdentifierValue(testIdentifierDefinition, testValue)
                .And
                .ContainSingleIdentifierValue(Identifiers.Definitions.NhsNumber, nhsNumber);
        }

        [Fact]
        public void GetAgencyFieldAndSubfields()
        {
            var field1 = createContext.Add(
                new IdentifierDefinitionEntity("first field", "field-1:composite(one:string,two:string)")).Entity;
            var agencyName = Random.String(15);
            createContext.Add(
                new AgencyEntity(Random.String(), agencyName)
                {
                    Fields = {new AgencyFieldEntity {Identifier = field1, Order = 1, Subfields = "one"}}
                });
            createContext.SaveChanges();

            var agency = repository.GetAgency(agencyName);

            agency.Fields.Should().BeEquivalentTo("field-1::one");
        }

        [Fact]
        public void GetsAgencyFieldInOrder()
        {
            var field1 = createContext.Add(new IdentifierDefinitionEntity("first field", "field-1:string")).Entity;
            var field2 = createContext.Add(new IdentifierDefinitionEntity("Second Field", "field-2:string")).Entity;
            var agencyEntity = new AgencyEntity("External Agency", "external")
            {
                Fields =
                {
                    new AgencyFieldEntity {Identifier = field2, Order = 1}
                }
            };
            createContext.Add(agencyEntity);
            createContext.SaveChanges();

            agencyEntity = updateContext.Set<AgencyEntity>().Find(agencyEntity.Id);
            agencyEntity.Fields.Add(
                new AgencyFieldEntity {Identifier = updateContext.IdentifierDefinition.Find(field1.Id), Order = 0});
            updateContext.SaveChanges();

            var agency = repository.GetAgency("external");

            using (new AssertionScope())
            {
                agency.Name.Should().Be("External Agency");
                agency.SystemName.Should().Be("external");
                agency.Fields.Should().ContainInOrder("field-1", "field-2");
            }
        }

        [Fact]
        public void GetsAnExistingPersonAgencyId()
        {
            var agencyEntity = new AgencyEntity("test", "test");
            updateContext.Add(agencyEntity);
            updateContext.SaveChanges();

            updateContext.Add(
                new PersonAgencyId {PersonId = personOne.Id, AgencyId = agencyEntity.Id, SpecificId = "AGENCY"});
            updateContext.SaveChanges();


            repository.GetPersonAgencyId((PersonIdentity) personOne.Id, (AgencyIdentity) agencyEntity.Id).Should()
                .Be("AGENCY");
        }

        [Fact]
        public void ReturnsNullForUnknownPerson() => Assert.Null(FindPersonByNhsNumber("UNKNOWN"));


        [Fact]
        public void UpdatesIdentifiersCorrectly()
        {
            var localRepository = CreateRepository(updateContext);
            var newTestValue = Random.String();
            localRepository.UpdatePerson(
                new PersonIdentity(personOne.Id),
                new[]
                {
                    Identifiers.PersonIdentifier(newTestValue, testIdentifierDefinition),
                    Identifiers.NhsNumber(personOneNhsNumber)
                },
                defaultAuthority.ToAuthority());
            updateContext.SaveChanges();


            var identifierEntities = readContext.PersonIdentifiers.Where(_ => _.Person.Id == personOne.Id).ToArray();

            identifierEntities.Should()
                .ContainSingleIdentifierValue(testIdentifierDefinition, personTwoNhsNumber, deleted: true)
                .And
                .ContainSingleIdentifierValue(testIdentifierDefinition, newTestValue)
                .And
                .ContainSingleIdentifierValue(Identifiers.Definitions.NhsNumber, personOneNhsNumber);
        }
    }
}