using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using Xunit;

namespace CHC.Consent.Tests.Identity
{
    public class IdentityRepositoryTests
    {
        [Fact]
        public void Test2()
        {
            var personOne = new Person {NhsNumber = "4333443"};
            var personTwo = new Person {NhsNumber = "6655666"};

            IdentityRepository repository = Create.AnIdentityRepository.WithPeople(personOne, personTwo);

            Assert.Equal(repository.FindPersonBy(Create.NhsNumber(personOne.NhsNumber)), personOne);
            Assert.Equal(repository.FindPersonBy(Create.NhsNumber(personTwo.NhsNumber)), personTwo);
            Assert.Null(repository.FindPersonBy(Create.NhsNumber("7787773")));
        }
        
        [Fact]
        public void Test3()
        {
            var personOne = new Person().WithBradfordHosptialNumbers("212925", "990099");
            var personTwo = new Person().WithBradfordHosptialNumbers("6655666");

            IdentityRepository repository = Create.AnIdentityRepository.WithPeople(personOne, personTwo);

            Assert.Equal(repository.FindPersonBy(Create.BradfordHospitalNumber("212925")), personOne);
            Assert.Equal(repository.FindPersonBy(Create.BradfordHospitalNumber("990099")), personOne);
            Assert.Equal(repository.FindPersonBy(Create.BradfordHospitalNumber("6655666")), personTwo);
            Assert.Null(repository.FindPersonBy(Create.BradfordHospitalNumber("7787773")));
            
        }

        [Fact]
        public void Test4()
        {
            var personOne = new Person{NhsNumber = "45"}.WithBradfordHosptialNumbers("7");
            
            IdentityRepository repository = Create.AnIdentityRepository.WithPeople(personOne);

            var found = repository.FindPersonBy(
                Create.NhsNumber(personOne.NhsNumber),
                Create.BradfordHospitalNumber("7")
            );
            
            Assert.Equal(personOne, found);
        }
    }
}