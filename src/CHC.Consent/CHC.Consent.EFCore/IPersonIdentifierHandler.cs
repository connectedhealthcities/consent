using System.Collections.Generic;
using System.Linq;
using CHC.Consent.Common.Identity;
using CHC.Consent.Common.Infrastructure.Data;
using CHC.Consent.EFCore.Entities;

namespace CHC.Consent.EFCore
{
    public interface IPersonIdentifierHandler
    {
        bool Update(PersonEntity person, IPersonIdentifier value, IStoreProvider stores);
        IEnumerable<IPersonIdentifier> Get(PersonEntity person, IStoreProvider stores);

        IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people, IPersonIdentifier identifier, IStoreProvider storeProvider);
    }

    public interface IPersonIdentifierHandler<TIdentifier> where TIdentifier : IPersonIdentifier
    {
        IQueryable<PersonEntity> Filter(
            IQueryable<PersonEntity> people, TIdentifier value, IStoreProvider stores);

        IEnumerable<TIdentifier> Get(PersonEntity person, IStoreProvider stores);
        
        bool Update(PersonEntity person, TIdentifier value, IStoreProvider stores);
    }
}