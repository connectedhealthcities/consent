using System;

namespace CHC.Consent.Common.Identity.IdentifierValues
{
    public class SexIdentifierValueType : IdentifierValueType<SexIdentifierValue>
    {
        /// <inheritdoc />
        public override SexIdentifierValue ParseToValue(string value)
        {
            if (Enum.TryParse<Common.Sex>(value, out var sex)) return new SexIdentifierValue(sex);
            throw new FormatException($"Cannot interpret '{value}' as Sex");
        }
    }
}