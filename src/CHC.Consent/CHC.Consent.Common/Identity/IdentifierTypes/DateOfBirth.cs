using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using CHC.Consent.Common.Identity.IdentifierValues;

namespace CHC.Consent.Common.Identity.IdentifierTypes
{
    public class DateOfBirth : SingleValueIdentifierType<DateIdentifierValue, DateIdentifierValueType, DateTime>
    {
        /// <inheritdoc />
        public DateOfBirth() : base("date-of-birth", _ => _.DateOfBirth)
        {
        }
    }
}