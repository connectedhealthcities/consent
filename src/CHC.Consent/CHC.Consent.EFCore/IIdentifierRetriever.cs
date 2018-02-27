using System.Collections.Generic;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    public interface IIdentifierRetriever<out TIdentifier> where TIdentifier : IIdentifier
    {
        IEnumerable<TIdentifier> Get(PersonEntity person, IStoreProvider stores);
    }
}