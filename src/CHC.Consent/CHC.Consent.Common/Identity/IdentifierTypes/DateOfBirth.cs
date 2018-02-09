using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using CHC.Consent.Common.Identity.IdentifierValues;

namespace CHC.Consent.Common.Identity.IdentifierTypes
{
    public class DateOfBirth : IdentifierType<DateIdentifierValue>
    {
        /// <inheritdoc />
        public DateOfBirth() : base("date-of-birth", canHaveMultipleValues:false, valueType:new DateIdentifierValueType())
        {
        }

        /// <inheritdoc />
        protected override Expression<Func<Person, bool>> GetMatchExpression(DateIdentifierValue value)
        {
            var dateOfBirth = value.Value;
            return p => p.DateOfBirth == dateOfBirth;
        }

        /// <inheritdoc />
        protected override void Update(Person person, DateIdentifierValue value)
        {
            if (person.DateOfBirth == DateTime.MinValue)
            {
                person.DateOfBirth = value.Value;
            }
            else if (person.DateOfBirth != value.Value)
            {
                throw new InvalidOperationException($"Cannot update Date Of Birth for Person#{person.Id}");
            }
        }
    }
}