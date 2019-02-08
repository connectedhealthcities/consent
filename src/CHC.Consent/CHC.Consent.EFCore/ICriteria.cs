using System.Linq;

namespace CHC.Consent.EFCore
{
    public interface ICriteria<T> where T : new()
    {
        IQueryable<T> ApplyTo(IQueryable<T> queryable, ConsentContext context);
    }
}