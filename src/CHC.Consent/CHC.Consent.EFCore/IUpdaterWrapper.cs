using CHC.Consent.Common;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    public interface IUpdaterWrapper
    {
        bool Update(PersonEntity person, IIdentifier value, IStoreProvider stores);
    }
}