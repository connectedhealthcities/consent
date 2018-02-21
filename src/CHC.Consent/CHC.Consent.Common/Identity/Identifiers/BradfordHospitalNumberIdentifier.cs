using System;
using System.Linq;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity.Identifiers
{
    [Identifier(TypeName, AllowMultipleValues = true)]
    public class BradfordHospitalNumberIdentifier : IIdentifier
    {
        public string Value { get; private set; }
        
        public BradfordHospitalNumberIdentifier(string value=null)
        {
            Value = value;
        }

        public Expression<Func<Person, bool>> GetMatchExpression()
        {
            var hospitalNumber = Value;
            return p => p.BradfordHospitalNumbers.Any(h => h == hospitalNumber);
        }

        public void Update(Person person)
        {
            person.AddHospitalNumber(Value);
        }

        public const string TypeName = "uk.nhs.bradfordhospitals.hosptial-id";
    }
}