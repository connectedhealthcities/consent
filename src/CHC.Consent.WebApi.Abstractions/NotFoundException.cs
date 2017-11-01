using System;

namespace CHC.Consent.WebApi.Abstractions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}