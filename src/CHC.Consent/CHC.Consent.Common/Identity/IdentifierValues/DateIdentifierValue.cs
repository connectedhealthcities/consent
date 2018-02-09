using System;

namespace CHC.Consent.Common.Identity.IdentifierValues
{
    public class DateIdentifierValue : IdentifierValue
    {
        public DateIdentifierValue(DateTime parsed)
        {
            Value = parsed.Date;
        }

        public DateTime Value { get; }
    }
}