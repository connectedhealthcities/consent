using System;
using System.Linq.Expressions;
using CHC.Consent.Common.Identity.IdentifierValues;

namespace CHC.Consent.Common.Identity.IdentifierTypes
{
    public class NhsNumber : SingleValueIdentifierType<StringIdentifierValue, StringIdentifierValueType, string>
    {
        /// <inheritdoc />
        public NhsNumber() :  base( "nhs.uk/nhs-number", _ => _.NhsNumber )
        {
        }
    }
}