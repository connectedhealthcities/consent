using System;

namespace CHC.Consent.Common.Identity
{
    public interface IPersonIdentifierDisplayHandlerProvider
    {
        IPersonIdentifierDisplayHandler GetDisplayHandler(IPersonIdentifier identifier);
        IPersonIdentifierDisplayHandler GetDisplayHandler(Type identifierType);
        IPersonIdentifierDisplayHandler GetDisplayHandler(string identifierName);
    }
}