using System;
using System.Linq;
using CHC.Consent.Common.Identity.Identifiers;
using CHC.Consent.EFCore.Entities;
using CHC.Consent.EFCore.IdentifierAdapters;
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
            for (var i = 0; i < 5; ++i)
            {
                AddPersonWithAHospitalNumber();
            }
            var personWithCorrectHosptialNumber = AddPersonWithAHospitalNumber("HOSPITAL NUMBER");
            
            var foundPeople = adapter.Filter(
                Context.People,
                new BradfordHospitalNumberIdentifier("HOSPITAL NUMBER"),
                new ContextWrapper(Context))
                .ToArray();

            var found = Assert.Single(foundPeople);
            Assert.NotNull(found);
            Assert.Equal(personWithCorrectHosptialNumber.Id, found.Id);
        }

        [Fact]
        public void CorrectlyAddsAHosptialNumber()
        {
            var person = AddPersonWithAHospitalNumber();

            adapter.Update(person, new BradfordHospitalNumberIdentifier("86"), new ContextWrapper(Context));
            Context.SaveChanges();

            var hospitalNumbers = otherContext.Set<BradfordHospitalNumberEntity>().Where(_ => _.Person.Id == person.Id)
                .ToArray();

            Assert.Single(hospitalNumbers, _ => _.HospitalNumber == "86");
        }

        private PersonEntity AddPersonWithAHospitalNumber(string hospitalNumber=null)
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