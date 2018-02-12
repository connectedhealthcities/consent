using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using CHC.Consent.Common.Identity.IdentifierValues;

namespace CHC.Consent.Common.Identity.IdentifierTypes
{
    public class DateOfBirth : SingleValueIdentifierType<DateIdentifierValue, DateIdentifierValueType, DateTime>
    {
        public const string TypeName = "date-of-birth";

        /// <inheritdoc />
        public DateOfBirth() : base(TypeName, _ => _.DateOfBirth)
        {
        }
        
        
    }
}