using System;
using System.Linq;
using System.Linq.Expressions;

namespace CHC.Consent.Common.Identity
{
    public class BradfordHospitalNumber : IdentifierType<IdentifierStringValue>
    {
        /// <inheritdoc />
        public BradfordHospitalNumber() : 
            base(
                "bradfordhospitals.nhs.uk/hosptial-id", 
                canHaveMultipleValues:true, 
                valueType:new IdentifierStringValueType())
        {
        }

        /// <inheritdoc />
        protected override Expression<Func<Person, bool>> GetMatchExpression(IdentifierStringValue value)
        {
            var hospitalNumber = value.Value;
            return p => p.BradfordHosptialNumbers.Any(h => h == hospitalNumber);
        }

        /// <inheritdoc />
        protected override void Update(Person person, IdentifierStringValue value)
        {
            person.AddHospitalNumber(value.Value);
        }
    }
}