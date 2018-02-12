using System;
using System.Linq.Expressions;
using CHC.Consent.Common.Identity.IdentifierValues;

namespace CHC.Consent.Common.Identity.IdentifierTypes
{
    public class NhsNumber : SingleValueIdentifierType<StringIdentifierValue, StringIdentifierValueType, string>
    {
        public const string TypeName = "nhs.uk/nhs-number";

        /// <inheritdoc />
        public NhsNumber() :  base( TypeName, _ => _.NhsNumber )
        {
        }
    }
}