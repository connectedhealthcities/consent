using System;
using System.Linq.Expressions;
using CHC.Consent.Common.Identity.IdentifierValues;

namespace CHC.Consent.Common.Identity.IdentifierTypes
{
    public class BirthOrderIdentifierType : SingleValueIdentifierType<UShortIdentifierValue, UShortIdentifierValueType, ushort?>
    {
        /// <inheritdoc />
        public BirthOrderIdentifierType() : base("birth-order", _ => _.BirthOrder)
        {
        }
    }
}