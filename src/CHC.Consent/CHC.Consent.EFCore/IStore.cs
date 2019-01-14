using System.Linq;

namespace CHC.Consent.EFCore
{
    //TODO: Replace with DbSet as this abstraction adds nothing to mix
    public interface IStore<T> : IQueryable<T> where T : IEntity
    {
        T Add(T value);
        T Get(long id);
    }
}