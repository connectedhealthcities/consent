using System;
using System.Globalization;

namespace CHC.Consent.Common.Identity
{
    public class IdentifierDateValueType : IdentifierValueType
    {
        /// <inheritdoc />
        public override IdentifierValue Parse(string value)
        {
            if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var parsed))
            {
                return new IdentifierDateValue(parsed);
            }
            throw new FormatException($"Cannot parse {value} to date");
        }
    }
}