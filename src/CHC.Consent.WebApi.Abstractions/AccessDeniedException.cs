using System;

namespace CHC.Consent.WebApi.Abstractions
{
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException(string message) : base(message)
        {
        }
    }
}