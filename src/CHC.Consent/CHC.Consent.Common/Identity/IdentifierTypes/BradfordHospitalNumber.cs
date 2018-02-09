using System;
using System.Linq;
using System.Linq.Expressions;
using CHC.Consent.Common.Identity.IdentifierValues;

namespace CHC.Consent.Common.Identity.IdentifierTypes
{
    public class BradfordHospitalNumber : IdentifierType<StringIdentifierValue>
    {
        /// <inheritdoc />
        public BradfordHospitalNumber() : 
            base(
                "bradfordhospitals.nhs.uk/hosptial-id", 
                canHaveMultipleValues:true, 
                valueType:new StringIdentifierValueType())
        {
        }

        /// <inheritdoc />
        protected override Expression<Func<Person, bool>> GetMatchExpression(StringIdentifierValue value)
        {
            var hospitalNumber = value.Value;
            return p => p.BradfordHosptialNumbers.Any(h => h == hospitalNumber);
        }

        /// <inheritdoc />
        protected override void Update(Person person, StringIdentifierValue value)
        {
            person.AddHospitalNumber(value.Value);
        }
    }
}