using System;

namespace CHC.Consent.Utils
{
    public class UtcSystemClock : IClock
    {
        /// <inheritdoc />
        public DateTimeOffset CurrentDateTimeOffset() => DateTimeOffset.UtcNow;

        /// <inheritdoc />
        public DateTime CurrentDateTime() => DateTime.UtcNow;
    }
}