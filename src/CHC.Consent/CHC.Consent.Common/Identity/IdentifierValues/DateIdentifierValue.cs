using System;
using CHC.Consent.Common.Identity.IdentifierTypes;

namespace CHC.Consent.Common.Identity.IdentifierValues
{
    public class DateIdentifierValue : IdentifierValue, IIdentifierValue<DateTime>
    {
        public DateIdentifierValue(DateTime parsed)
        {
            Value = parsed.Date;
        }

        public DateTime Value { get; }
    }
}