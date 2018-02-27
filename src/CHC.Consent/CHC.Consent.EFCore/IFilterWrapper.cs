using System.Linq;
using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    public interface IFilterWrapper
    {
        IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people, IIdentifier identifier, IStoreProvider storeProvider);
    }
}