using System;

namespace CHC.Consent.Utils
{
    public interface IClock
    {
        DateTimeOffset CurrentDateTimeOffset();
        DateTime CurrentDateTime();
    }
}