using System.Linq;
using CHC.Consent.Common.Infrastructure.Data;

namespace CHC.Consent.Common.Identity
{
    public interface IQueryableProvider
    {
        IQueryable<T> Get<T>() where T : class;
    }

    public interface IStoreProvider
    {
        IStore<T> Get<T>() where T : class, IEntity;
    }
    
    public interface IIdentifierFilter<in TIdentifier> where TIdentifier:IIdentifier
    {
        IQueryable<TPerson> Filter<TPerson>(IQueryable<TPerson> people, TIdentifier value, IStoreProvider stores) where TPerson:Person;
    }

    public interface IIdentifierUpdater<in TIdentifier> where TIdentifier:IIdentifier
    {
        bool Update(Person person, TIdentifier value, IStoreProvider stores);
    }
}