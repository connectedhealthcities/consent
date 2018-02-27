using System.Collections.Generic;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    public interface IRetrieverWrapper
    {
        IEnumerable<IIdentifier> Get(PersonEntity person, IStoreProvider stores);
    }
}