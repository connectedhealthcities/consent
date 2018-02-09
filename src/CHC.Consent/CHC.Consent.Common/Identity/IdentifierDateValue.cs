using System;

namespace CHC.Consent.Common.Identity
{
    public class IdentifierDateValue : IdentifierValue
    {
        public IdentifierDateValue(DateTime parsed)
        {
            Value = parsed.Date;
        }

        public DateTime Value { get; }
    }
}