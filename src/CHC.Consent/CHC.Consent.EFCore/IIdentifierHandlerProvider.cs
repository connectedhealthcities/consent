using System;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Identity.Identifiers;

namespace CHC.Consent.EFCore
{
    public interface IIdentifierHandlerProvider
    {
        IPersonIdentifierDisplayHandler GetDisplayHandler(IdentifierDefinition identifierType);
    }
}