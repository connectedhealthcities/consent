using CHC.Consent.Common;
using Xunit;

namespace CHC.Consent.Tests
{
    public class CreateTests
    {
        public class PersonBuilderTests
        {
            [Fact]
            public void ANewPersonShouldHaveNoHospitalNumbers()
            {
                var person = (Person) Create.Person;
                
                Assert.Empty(person.BradfordHospitalNumbers);
            }
        }
    }
}