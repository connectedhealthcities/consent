using System;

namespace CHC.Consent.EFCore
{
    public interface IIdentifierHandlerProvider
    {
        IPersonIdentifierHandler GetHandler(Type identifierType);
    }
}