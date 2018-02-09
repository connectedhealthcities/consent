using System;
using System.Linq.Expressions;
using CHC.Consent.Common.Identity.IdentifierValues;

namespace CHC.Consent.Common.Identity.IdentifierTypes
{
    public class NhsNumber : IdentifierType<StringIdentifierValue>
    {
        /// <inheritdoc />
        public NhsNumber() : 
            base(
                "nhs.uk/nhs-number", 
                canHaveMultipleValues:false, 
                valueType:new StringIdentifierValueType())
        {
        }

        /// <inheritdoc />
        protected override Expression<Func<Person, bool>> GetMatchExpression(StringIdentifierValue value)
        {
            var nhsNumber = value.Value;
            return p => p.NhsNumber == nhsNumber;
        }

        /// <inheritdoc />
        protected override void Update(Person person, StringIdentifierValue value)
        {
            if (string.IsNullOrEmpty(person.NhsNumber))
            {
                person.NhsNumber = value.Value;
            }
            else if (person.NhsNumber != value.Value)
            {
                throw new InvalidOperationException($"Cannot update NhsNumber on Person#{person.Id}");
            }
        }
    }
}