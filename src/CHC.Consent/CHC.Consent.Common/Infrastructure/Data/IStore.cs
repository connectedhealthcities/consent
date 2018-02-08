using System.Linq;

namespace CHC.Consent.Common.Infrastructure.Data
{
    public interface IStore<T> : IQueryable<T>
    {
        T Add(T value);
    }
}