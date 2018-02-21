using System.Linq;

namespace CHC.Consent.Common.Infrastructure.Data
{
    public interface IStore<T> : IQueryable<T> where T : IEntity
    {
        T Add(T value);
        T Get(long id);
    }
}