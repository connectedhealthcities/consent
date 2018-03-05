using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    public interface IIdentifierFilter<in TIdentifier> where TIdentifier:IPersonIdentifier
    {
        IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people, TIdentifier value, IStoreProvider stores);
    }
}