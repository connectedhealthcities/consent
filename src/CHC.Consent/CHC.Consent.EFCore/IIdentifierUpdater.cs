using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    public interface IIdentifierUpdater<in TIdentifier> where TIdentifier:IIdentifier
    {
        bool Update(PersonEntity person, TIdentifier value, IStoreProvider stores);
    }
}