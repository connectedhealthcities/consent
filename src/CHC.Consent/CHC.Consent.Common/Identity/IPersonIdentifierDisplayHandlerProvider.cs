using System;

namespace CHC.Consent.Common.Identity
{
    public interface IPersonIdentifierDisplayHandlerProvider
    {
        IPersonIdentifierDisplayHandler GetDisplayHandler(string identifierName);
    }
}