using System;
using System.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.IdentifierAdapters;
using CHC.Consent.Testing.Utils;
using Xunit;
using Xunit.Abstractions;
using Random = CHC.Consent.Testing.Utils.Random;

namespace CHC.Consent.EFCore.Tests.IdentifierAdapterTests
{
    [Collection(DatabaseCollection.Name)]
    public class BradfordHospitalNumberIdentifierAdapterTests : DbTests
    {
        private readonly ConsentContext saveContext;
        private ConsentContext readContext;

        private readonly BradfordHospitalNumberIdentifierAdapter adapter 
            = new BradfordHospitalNumberIdentifierAdapter();

        /// <inheritdoc />
        public BradfordHospitalNumberIdentifierAdapterTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            saveContext = CreateNewContextInSameTransaction();
            readContext = CreateNewContextInSameTransaction();
        }

        [Fact]
        public void CorrectlyFiltersAList()
        {
            var personWithCorrectHosptialNumber = AddPersonWithAHospitalNumber("HOSPITAL NUMBER");
            5.Times(AddPersonWithAHospitalNumber);
            saveContext.SaveChanges();
            
            var foundPeople = adapter.Filter(
                readContext.People,
                new BradfordHospitalNumberIdentifier("HOSPITAL NUMBER"),
                new ContextStoreProvider(readContext))
                .ToArray();

            var found = Assert.Single(foundPeople);
            Assert.NotNull(found);
            Assert.Equal(personWithCorrectHosptialNumber.Id, found.Id);
        }

        [Fact]
        public void CorrectlyAddsAHosptialNumber()
        {
            var oldHospitalNumber = Random.String();
            var person = AddPersonWithAHospitalNumber(oldHospitalNumber);
            saveContext.SaveChanges();

            var newHospitalNumber = Random.String();
            adapter.Update(
                Context.Find<PersonEntity>(person.Id),
                new BradfordHospitalNumberIdentifier(newHospitalNumber),
                new ContextStoreProvider(Context));
            Context.SaveChanges();

            var hospitalNumbers = readContext.Set<PersonIdentifierEntity>()
                .Where(_ => _.Person.Id == person.Id && _.TypeName == BradfordHospitalNumberIdentifier.TypeName)
                .OrderBy(_ => _.Created)
                .ToArray();

            Assert.Collection(hospitalNumbers,
                _ => { Assert.NotNull(_.Deleted); Assert.Equal(oldHospitalNumber, _.Value); },
                _ => { Assert.Null(_.Deleted); Assert.Equal(newHospitalNumber, _.Value);});
        }

        private PersonEntity AddPersonWithAHospitalNumber() => AddPersonWithAHospitalNumber(Random.String());
        private PersonEntity AddPersonWithAHospitalNumber(string hospitalNumber)
        {
            var person = saveContext.Add(new PersonEntity()).Entity;
            saveContext.Add(
                new PersonIdentifierEntity
                {
                    Person = person,
                    TypeName = BradfordHospitalNumberIdentifier.TypeName,
                    Value = hospitalNumber,
                    ValueType = "string"
                });
            
            return person;
        }
    }
}