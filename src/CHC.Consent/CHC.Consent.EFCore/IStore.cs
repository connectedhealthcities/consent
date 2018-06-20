using System.Linq;

namespace CHC.Consent.EFCore
{
    public interface IStore<T> : IQueryable<T> where T : IEntity
    {
        T Add(T value);
        T Get(long id);
    }
}