using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.Common.Infrastructure;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.Testing.Utils;
using FakeItEasy;
using FakeItEasy.Sdk;
using FluentAssertions;
using FluentAssertions.Collections;
using Microsoft.VisualBasic.CompilerServices;
using Xunit;
using Xunit.Abstractions;
using Random = CHC.Consent.Testing.Utils.Random;

namespace CHC.Consent.EFCore.Tests
{
    public class IdentityRepositoryTests : DbTests
    {
        private readonly string personOneNhsNumber = Random.String();
        private readonly string personTwoNhsNumber = Random.String();
        private readonly PersonEntity personOne;
        private readonly PersonEntity personTwo;
        private readonly IdentityRepository repository;
        private readonly IdentifierDefinition testIdentifierDefinition;

        private PersonIdentity FindPersonByNhsNumber(string nhsNumber)
        {
            return repository.FindPersonBy(Identifiers.NhsNumber(nhsNumber));
        }

        private PersonIdentifierEntity NhsNumberEntity(PersonEntity personEntity, string nhsNumber)
        {
            return IdentifierEntity(personEntity, Identifiers.Definitions.NhsNumber, nhsNumber);
        }

        private static PersonIdentifierEntity IdentifierEntity(
            PersonEntity personEntity, IdentifierDefinition identifierDefinition, string value)
        {
            return new PersonIdentifierEntity
            {
                Person = personEntity,
                TypeName = identifierDefinition.SystemName,
                Value = MarshallValue(identifierDefinition, value),
                ValueType = identifierDefinition.Type.SystemName
            };
        }

        private static string MarshallValue(IdentifierDefinition identifierDefinition, string value)
        {
            return new IdentifierToXmlElementMarshaller(identifierDefinition)
                .MarshallToXml(Identifiers.PersonIdentifier(value, identifierDefinition))
                .ToString(SaveOptions.DisableFormatting);
        }

        /// <inheritdoc />
        public IdentityRepositoryTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            personOne = new PersonEntity();
            personTwo = new PersonEntity();
            
            Context.AddRange(personOne, personTwo);
            testIdentifierDefinition = Identifiers.Definitions.String("Test");
            Context.AddRange(
                NhsNumberEntity(personOne, personOneNhsNumber), 
                NhsNumberEntity(personTwo, personTwoNhsNumber),
                IdentifierEntity(personOne, testIdentifierDefinition, personTwoNhsNumber)
                );
            Context.SaveChanges();
            
            Assert.NotNull(readContext.People.Find(personOne.Id));
            Assert.NotNull(readContext.People.Find(personTwo.Id));
            Assert.Equal(2, readContext.Set<PersonIdentifierEntity>().Count(_ => _.Person == personOne));
            Assert.Single(readContext.Set<PersonIdentifierEntity>().Where(_ => _.Person == personTwo));

            repository = CreateRepository(CreateNewContextInSameTransaction());
        }

        private IdentityRepository CreateRepository(ConsentContext context)
        {
            IDictionary<string, IIdentifierMarshaller> marshallers = new Dictionary<string, IIdentifierMarshaller>
            {
                [Identifiers.Definitions.NhsNumber.SystemName] =
                    new IdentifierToXmlElementMarshaller(Identifiers.Definitions.NhsNumber),
                [testIdentifierDefinition.SystemName] = new IdentifierToXmlElementMarshaller(testIdentifierDefinition)
            };

            IStoreProvider storeProvider = new ContextStoreProvider(context);

            return new IdentityRepository(
                marshallers,
                storeProvider);
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
                    
                });
            updateContext.SaveChanges();
            

            var identifierEntities = readContext.PersonIdentifiers.Where(_ => _.Person.Id == personOne.Id).ToArray();

            identifierEntities.Should()
                .ContainSingleIdentifierValue(testIdentifierDefinition, personTwoNhsNumber, deleted: true)
                .And
                .ContainSingleIdentifierValue(testIdentifierDefinition, newTestValue)
                .And
                .ContainSingleIdentifierValue(Identifiers.Definitions.NhsNumber, personOneNhsNumber);

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
                });
            updateContext.SaveChanges();

            var personIdentifiers = CreateRepository(readContext).GetPersonIdentifiers(personOne.Id).ToArray();

            personIdentifiers.Should().HaveCount(2, "because there are two active identifiers");


            personIdentifiers.Should().ContainSingle(
                _ => Equals(_.Value.Value, personOneNhsNumber) && _.Definition == Identifiers.Definitions.NhsNumber);


            personIdentifiers.Should().ContainSingle(
                _ => Equals(_.Value.Value, newTestValue) && _.Definition == testIdentifierDefinition);
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
                }
            );

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
    }

    public static class PersonIdentifierCollectionAssertions
    {
        public static AndWhichConstraint<GenericCollectionAssertions<PersonIdentifierEntity>, PersonIdentifierEntity>
            ContainSingleIdentifierValue(
                this GenericCollectionAssertions<PersonIdentifierEntity> assertions,
                IdentifierDefinition definition,
                string value,
                bool deleted = false
            ) => assertions.ContainSingle(_ => _.Value == MarshallValue(definition, value) && _.TypeName == definition.SystemName && (deleted?_.Deleted != null:_.Deleted == null));

        private static string MarshallValue(IdentifierDefinition definition, string value)
        {
            return definition.CreateMarshaller()
                .MarshallToXml(Identifiers.PersonIdentifier(value, definition))
                .ToString(SaveOptions.DisableFormatting);
        }

        public static IIdentifierMarshaller CreateMarshaller(this IdentifierDefinition definition)
        {
            var creator = new IdentifierMarshallerCreator(new Dictionary<string, IIdentifierMarshaller>());
            definition.Accept(creator);
            var marshaller = creator.Marshallers.Values.Single();
            return marshaller;
        }
    }
}