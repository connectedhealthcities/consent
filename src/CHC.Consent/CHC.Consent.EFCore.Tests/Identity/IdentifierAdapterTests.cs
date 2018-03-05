using System;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.Identity;
using CHC.Consent.Testing.Utils;
using Xunit;
using Xunit.Abstractions;
using Random = CHC.Consent.Testing.Utils.Random;


namespace CHC.Consent.EFCore.Tests.Identity
{
    public class IdentifierAdapterTests : DbTests
    {
        private const string IdentifierTypeName = "Testing";
        public const string ValueTypeName = "chc.testing";
        private readonly PersonIdentifierHandler<Identifier> handler;
        private ConsentContext creatingContext;
        private ConsentContext updatingContext;
        private ConsentContext readingContext;
        private readonly PersonEntity personEntity;
        private readonly IdentifierMarshaller marshaller;


        /// <inheritdoc />
        public IdentifierAdapterTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            marshaller = new IdentifierMarshaller();
            handler = new PersonIdentifierHandler<Identifier>(marshaller, IdentifierTypeName);
            creatingContext = CreateNewContextInSameTransaction();
            updatingContext = CreateNewContextInSameTransaction();
            readingContext = CreateNewContextInSameTransaction();
            
            personEntity = creatingContext.Add(new PersonEntity()).Entity;
            creatingContext.SaveChanges();
        }

        [Identifier(name: IdentifierTypeName)]
        class Identifier : IPersonIdentifier
        {
            public string Value { get; set; }
        }

        class IdentifierMarshaller : IIdentifierMarshaller<Identifier>
        {
            /// <inheritdoc />
            public string ValueType => ValueTypeName;

            /// <inheritdoc />
            public string MarshalledValue(Identifier value) => value?.Value;

            /// <inheritdoc />
            public Identifier Unmarshall(string valueType, string value) =>
                value == null ? null : new Identifier() {Value = value};

        }

        [Fact]
        public void SavesNewIdentifierWhenNoneExists()
        {
            var identifier = new Identifier {Value = "blah"};
            var updated = Update(identifier);
            
            
            var identifiers = GetIdentifierEntities();

            Assert.True(updated);
            
            AssertActiveIdentifier(marshaller.MarshalledValue(identifier), Assert.Single(identifiers));
        }

        [Fact]
        public void DoesNotUpdateAnythingWhenIdentifierValueDoesNotChange()
        {
            Update(new Identifier {Value = "A Value"}, context: CreateNewContextInSameTransaction());

            var updated = Update(new Identifier {Value = "A Value"});

            var identifiers = GetIdentifierEntities();
            
            Assert.False(updated);

            AssertActiveIdentifier("A Value", Assert.Single(identifiers));
        }

        [Fact]
        public void ChangingIdentifierValue_SavesNewIdentifier_MarksExistingIdentifiersAsDeleted()
        {
            Update(new Identifier {Value = "Old Value"}, context: CreateNewContextInSameTransaction());

            var updated = Update(new Identifier {Value = "New Value"});

            var identifiers = GetIdentifierEntities();

            Assert.True(updated);

            Assert.Collection(
                identifiers,
                ElementIsDeletedIdentifier("Old Value"),
                ElementIsActiveIdentifier("New Value")
            );
        }

        [Fact]
        public void CanFindPersonByIdentifierValue()
        {
            5.Times(AddPersonWithRandomIdentifier);

            Update(new Identifier {Value = "find me"}, personEntity);
            creatingContext.SaveChanges();


            var foundPeople = handler.Filter(
                readingContext.People,
                new Identifier {Value = "find me"},
                new ContextStoreProvider(readingContext))
                .ToArray();

            
            Assert.Equal(personEntity, Assert.Single(foundPeople));
        }
        
        
        [Fact]
        public void CanRetreveIdentifiersForPerson()
        {
            5.Times(AddPersonWithRandomIdentifier);

            Update(new Identifier {Value = "find me"}, personEntity);
            creatingContext.SaveChanges();


            var retrievedIdentifiers = handler.Get(personEntity, new ContextStoreProvider(readingContext)).ToArray();


            var identifier = Assert.Single(retrievedIdentifiers);
            Assert.NotNull(identifier);
            Assert.Equal("find me", identifier.Value);
        }

        private bool AddPersonWithRandomIdentifier()
        {
            var person = creatingContext.Add(new PersonEntity()).Entity;
            return Update(new Identifier {Value = Random.String()}, person, creatingContext);
        }

        private Action<PersonIdentifierEntity> ElementIsActiveIdentifier(string expectedValue) => 
            i => AssertActiveIdentifier(expectedValue, i);

        private Action<PersonIdentifierEntity> ElementIsDeletedIdentifier(string expectedValue) =>
            i => AssertDeletedIdentifier(expectedValue, i);

        private PersonIdentifierEntity[] GetIdentifierEntities(PersonEntity person=null)
        {
            person = person ?? personEntity;
            return readingContext.Set<PersonIdentifierEntity>().Where(_ => _.Person.Id == person.Id)
                .OrderBy(_ => _.Created)
                .ToArray();
        }

        private void AssertActiveIdentifier(string expectedValue, PersonIdentifierEntity identifier)
        {
            AssertIdentifier(expectedValue, identifier);
            Assert.Null(identifier.Deleted);
        }

        private void AssertDeletedIdentifier(string expectedValue, PersonIdentifierEntity identifier)
        {
            AssertIdentifier(expectedValue, identifier);
            Assert.NotNull(identifier.Deleted);
        }

        private static void AssertIdentifier(string expectedValue, PersonIdentifierEntity identifier)
        {
            Assert.NotNull(identifier);
            Assert.Equal(expectedValue, identifier.Value);
            Assert.Equal(IdentifierTypeName, identifier.TypeName);
            Assert.Equal(ValueTypeName, identifier.ValueType);
        }


        private bool Update(Identifier identifier, PersonEntity person=null, ConsentContext context = null)
        {
            context = context ?? updatingContext;
            person = person ?? personEntity;
            var updated = handler.Update(
                context.Find<PersonEntity>(person.Id),
                identifier,
                new ContextStoreProvider(context));
            context.SaveChanges();
            return updated;
        }
    }

    
}