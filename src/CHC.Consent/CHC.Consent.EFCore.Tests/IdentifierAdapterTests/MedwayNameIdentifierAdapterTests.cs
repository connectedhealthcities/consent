using System.Linq;
using CHC.Consent.Common.Identity.Identifiers.Medway;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.IdentifierAdapters;
using CHC.Consent.Testing.Utils;
using Xunit;
using Xunit.Abstractions;

namespace CHC.Consent.EFCore.Tests.IdentifierAdapterTests
{
    public class MedwayNameIdentifierAdapterTests : DbTests
    {
        private readonly ConsentContext saveContext;
        private readonly ConsentContext readContext;
        
        /// <inheritdoc />
        public MedwayNameIdentifierAdapterTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            saveContext = CreateNewContextInSameTransaction();
            readContext = CreateNewContextInSameTransaction();
        }

        [Fact]
        public void DoesNotFindPersonWithDifferentName()
        {
            var person = saveContext.People.Add(new PersonEntity()).Entity;
            var firstName = Random.String();
            var lastName = Random.String();
            saveContext.Set<MedwayNameEntity>().Add(
                new MedwayNameEntity {Person = person, FirstName = firstName, LastName = lastName});
            saveContext.SaveChanges();
            
            var identifier = new MedwayNameIdentifier { FirstName = Random.String(), LastName = Random.String()};
            
            var adapter = new MedwayNameIdentifierAdapter();
            var foundPeople = adapter.Filter(
                    readContext.People,
                    identifier,
                    new ContextWrapper(readContext))
                .ToArray();
            
            Assert.Empty(foundPeople);
        }

        [Fact]
        public void CanFindPersonWithMatchingName()
        {
            var person = saveContext.People.Add(new PersonEntity()).Entity;
            var firstName = Random.String();
            var lastName = Random.String();
            saveContext.Set<MedwayNameEntity>().Add(
                new MedwayNameEntity {Person = person, FirstName = firstName, LastName = lastName});
            saveContext.SaveChanges();
            

            var adapter = new MedwayNameIdentifierAdapter();

            var foundPeople = adapter.Filter(
                readContext.People,
                new MedwayNameIdentifier {FirstName = firstName, LastName = lastName},
                new ContextWrapper(readContext))
                .ToArray();

            var found = Assert.Single(foundPeople);
            Assert.NotNull(found);
            Assert.Equal(person.Id, found.Id);
        }

        [Fact]
        public void CreatesANameWhenNoneExists()
        {
            var person = saveContext.People.Add(new PersonEntity()).Entity;
            saveContext.SaveChanges();
            
            var adapter = new MedwayNameIdentifierAdapter();

            var identifier = new MedwayNameIdentifier { FirstName = Random.String(), LastName = Random.String()};
            adapter.Update(person, identifier, new ContextWrapper(Context));
            Context.SaveChanges();

            var foundNames = readContext.Set<MedwayNameEntity>().WherePersonIs(person).ToArray();

            var newName = Assert.Single(foundNames);
            Assert.NotNull(newName);
            Assert.Equal(identifier.FirstName, newName.FirstName);
            Assert.Equal(identifier.LastName, newName.LastName);
        }
        
        
        [Fact]
        public void UpdatesAnExistingName()
        {
            var person = saveContext.People.Add(new PersonEntity()).Entity;
            var firstName = Random.String();
            var lastName = Random.String();
            saveContext.Set<MedwayNameEntity>().Add(
                new MedwayNameEntity {Person = person, FirstName = firstName, LastName = lastName});
            saveContext.SaveChanges();
            
            var adapter = new MedwayNameIdentifierAdapter();

            var newNameIdentifier = new MedwayNameIdentifier { FirstName = Random.String(), LastName = Random.String()};
            adapter.Update(person, newNameIdentifier, new ContextWrapper(Context));
            Context.SaveChanges();

            var foundNames = readContext.Set<MedwayNameEntity>().WherePersonIs(person).ToArray();

            var newName = Assert.Single(foundNames);
            Assert.NotNull(newName);
            Assert.Equal(newNameIdentifier.FirstName, newName.FirstName);
            Assert.Equal(newNameIdentifier.LastName, newName.LastName);
        }
    }
}