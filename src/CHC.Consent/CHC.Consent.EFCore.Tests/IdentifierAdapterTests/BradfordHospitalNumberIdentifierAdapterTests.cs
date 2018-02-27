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
        private readonly ConsentContext otherContext;

        /// <inheritdoc />
        public BradfordHospitalNumberIdentifierAdapterTests(ITestOutputHelper outputHelper, DatabaseFixture fixture) : base(outputHelper, fixture)
        {
            otherContext = CreateNewContextInSameTransaction();
        }

        private readonly BradfordHospitalNumberIdentifierAdapter adapter 
            = new BradfordHospitalNumberIdentifierAdapter();

        [Fact]
        public void CorrectlyFiltersAList()
        {
            5.Times(AddPersonWithAHospitalNumber);
            
            var personWithCorrectHosptialNumber = AddPersonWithAHospitalNumber("HOSPITAL NUMBER");
            
            var foundPeople = adapter.Filter(
                Context.People,
                new BradfordHospitalNumberIdentifier("HOSPITAL NUMBER"),
                new ContextStoreProvider(Context))
                .ToArray();

            var found = Assert.Single(foundPeople);
            Assert.NotNull(found);
            Assert.Equal(personWithCorrectHosptialNumber.Id, found.Id);
        }

        [Fact]
        public void CorrectlyAddsAHosptialNumber()
        {
            var person = AddPersonWithAHospitalNumber();

            adapter.Update(person, new BradfordHospitalNumberIdentifier("86"), new ContextStoreProvider(Context));
            Context.SaveChanges();

            var hospitalNumbers = otherContext.Set<BradfordHospitalNumberEntity>().Where(_ => _.Person.Id == person.Id)
                .ToArray();

            Assert.Equal(2, hospitalNumbers.Length);
            Assert.Single(hospitalNumbers, _ => _.HospitalNumber == "86");
        }

        private PersonEntity AddPersonWithAHospitalNumber() => AddPersonWithAHospitalNumber(Random.String());
        private PersonEntity AddPersonWithAHospitalNumber(string hospitalNumber)
        {
            var person = otherContext.Add(new PersonEntity()).Entity;
            otherContext.Add(
                new BradfordHospitalNumberEntity
                {
                    Person = person,
                    HospitalNumber = hospitalNumber ?? Random.String()
                });
            otherContext.SaveChanges();
            return person;
        }
    }
}