using System;
using CHC.Consent.Common.Identity.IdentifierTypes;

namespace CHC.Consent.Common.Identity.IdentifierValues
{
    public class UShortIdentifierValueType : IdentifierValueType<UShortIdentifierValue>
    {
        /// <inheritdoc />
        public override UShortIdentifierValue ParseToValue(string value)
        {
            if (ushort.TryParse(value, out var parsed))
            {
                return new UShortIdentifierValue(parsed);
            }
            throw new FormatException($"Cannot parse {value} as ushort");
        }
    }
}