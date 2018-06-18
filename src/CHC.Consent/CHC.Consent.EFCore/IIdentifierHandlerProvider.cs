using System;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.EFCore
{
    public interface IIdentifierHandlerProvider
    {
        IPersonIdentifierPersistanceHandler GetPersistanceHandler(Type identifierType);
        IPersonIdentifierDisplayHandler GetDisplayHandler(Type identifierType);
    }
}