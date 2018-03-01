using System.Linq;
using System.Text;
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
        private readonly XmlIdentifierMarshaller<MedwayNameIdentifier> marshaller;

        /// <inheritdoc />
        public MedwayNameIdentifierAdapterTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            saveContext = CreateNewContextInSameTransaction();
            readContext = CreateNewContextInSameTransaction();
            marshaller = new XmlIdentifierMarshaller<MedwayNameIdentifier>(valueType: "BIB4All.MedwayName");
        }

       
        
        [Fact]
        public void DoesNotFindPersonWithDifferentName()
        {
            5.Times(AddPersonWithMedwayName);
            saveContext.SaveChanges();
            
            var identifier = new MedwayNameIdentifier { FirstName = Random.String(), LastName = Random.String()};
            
            var adapter = new MedwayNameIdentifierAdapter();
            var foundPeople = adapter.Filter(
                    readContext.People,
                    identifier,
                    new ContextStoreProvider(readContext))
                .ToArray();
            
            Assert.Empty(foundPeople);
        }

        [Fact]
        public void CanFindPersonWithMatchingName()
        {
            5.Times(AddPersonWithMedwayName);
            
            var firstName = Random.String();
            var lastName = Random.String();
            var person = AddPersonWithMedwayName(firstName, lastName);
            saveContext.SaveChanges();
            

            var adapter = new MedwayNameIdentifierAdapter();

            var foundPeople = adapter.Filter(
                readContext.People,
                new MedwayNameIdentifier {FirstName = firstName, LastName = lastName},
                new ContextStoreProvider(readContext))
                .ToArray();

            var found = Assert.Single(foundPeople);
            Assert.NotNull(found);
            Assert.Equal(person.Id, found.Id);
        }

        private PersonEntity AddPersonWithMedwayName() => AddPersonWithMedwayName(Random.String(), Random.String());
        
        
        private PersonEntity AddPersonWithMedwayName(string firstName, string lastName)
        {
            var person = saveContext.People.Add(new PersonEntity()).Entity;

            saveContext.Set<PersonIdentifierEntity>().Add(
                new PersonIdentifierEntity
                {
                    Person = person,
                    TypeName = MedwayNameIdentifier.TypeName,
                    ValueType = marshaller.ValueType,
                    Value = marshaller.MarshalledValue(new MedwayNameIdentifier(firstName, lastName))
                });

            return person;
        }

        [Fact]
        public void CreatesANameWhenNoneExists()
        {
            var person = saveContext.People.Add(new PersonEntity()).Entity;
            saveContext.SaveChanges();
            
            var adapter = new MedwayNameIdentifierAdapter();

            var identifier = new MedwayNameIdentifier { FirstName = Random.String(), LastName = Random.String()};
            adapter.Update(Context.People.Find(person.Id), identifier, new ContextStoreProvider(Context));
            Context.SaveChanges();

            var foundNames = GetNamesFor(person).Where(_ => _.Deleted == null).ToArray();

            var newName = Assert.Single(foundNames);
            Assert.NotNull(newName);
            Assert.Equal(marshaller.MarshalledValue(identifier), newName.Value);
        }

        private IQueryable<PersonIdentifierEntity> GetNamesFor(PersonEntity person)
        {
            return readContext.Set<PersonIdentifierEntity>().Where(_ => _.Person.Id == person.Id && _.TypeName == MedwayNameIdentifier.TypeName).OrderBy(_ => _.Created);
        }


        [Fact]
        public void UpdatesAnExistingName()
        {
            var firstName = Random.String();
            var lastName = Random.String();
            var person = AddPersonWithMedwayName(firstName, lastName);
            
            saveContext.SaveChanges();
            
            var adapter = new MedwayNameIdentifierAdapter();

            var newNameIdentifier = new MedwayNameIdentifier(Random.String(), Random.String());
            adapter.Update(Context.People.Find(person.Id), newNameIdentifier, new ContextStoreProvider(Context));
            Context.SaveChanges();

            var foundNames = GetNamesFor(person).ToArray();

            Assert.Collection(
                foundNames,
                _ =>
                {
                    Assert.NotNull(_.Deleted);
                    Assert.Equal(marshaller.MarshalledValue(new MedwayNameIdentifier(firstName, lastName)), _.Value);
                },
                _ =>
                {
                    Assert.Null(_.Deleted);
                    Assert.Equal(marshaller.MarshalledValue(newNameIdentifier), _.Value);
                }
            );
        }
    }
}