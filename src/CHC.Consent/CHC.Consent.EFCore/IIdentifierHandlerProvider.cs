using System;
using CHC.Consent.Common.Identity;

namespace CHC.Consent.EFCore
{
    public interface IIdentifierHandlerProvider
    {
        IFilterWrapper GetFilter(IPersonIdentifier identifier);
        IRetrieverWrapper GetRetriever(Type identifierType);
        IUpdaterWrapper GetUpdater(Type identifierType);
    }
}