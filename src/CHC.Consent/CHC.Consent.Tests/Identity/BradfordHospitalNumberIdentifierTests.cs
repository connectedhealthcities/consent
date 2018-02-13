using System;
using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class BradfordHospitalNumberIdentifierTests
    {
        private const string HospitalNumber = "2134";
        private readonly IIdentifier identifier;

        public BradfordHospitalNumberIdentifierTests()
        {
            identifier = new BradfordHospitalNumberIdentifier(HospitalNumber);
        }

        [Fact]
        public void CanUpdatePersonWithNoHostpialNumber()
        {
            var person = new Person();
            
            identifier.Update(person);
            
            Assert.Contains(HospitalNumber, person.BradfordHospitalNumbers);
        }
        
        
        [Fact]
        public void CanFilterPeopleByHosptialNumber()
        {
            
            var allPeople = new Person[]
            {
                Create.Person.WithHospitalNumbers("Ignored"),
                Create.Person.WithHospitalNumbers("Peter Crowther"),
                Create.Person.WithHospitalNumbers("Orange tips"),
                Create.Person.WithHospitalNumbers(HospitalNumber),
                Create.Person.WithHospitalNumbers("Blah Blah Blah"),
            }.AsQueryable();


            var matchExpression = identifier.GetMatchExpression();

            Assert.All(
                allPeople.Where(matchExpression),
                _ => Assert.Contains(HospitalNumber, _.BradfordHospitalNumbers));
        }
        
        
        [Fact]
        public void DoesNothingIfPersonAlreadyHasHostpialNumber()
        {
            Person person = Create.Person.WithHospitalNumbers(HospitalNumber);
            
            identifier.Update(person);
            
            Assert.Contains(HospitalNumber, person.BradfordHospitalNumbers);
        }
        
        [Fact]
        public void AddsMissingHospitalNumbers()
        {
            Person person = Create.Person.WithHospitalNumbers("NOT THE SAME");

            identifier.Update(person);
            
            Assert.Contains("NOT THE SAME", person.BradfordHospitalNumbers);
            Assert.Contains(HospitalNumber, person.BradfordHospitalNumbers);
        }
    }
}