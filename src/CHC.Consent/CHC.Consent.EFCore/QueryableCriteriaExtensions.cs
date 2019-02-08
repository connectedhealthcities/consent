using System.Linq;

namespace CHC.Consent.EFCore
{
    public static class QueryableCriteriaExtensions
    {
        public static IQueryable<T> Search<T>(
            this IQueryable<T> source, ConsentContext db, params ICriteria<T>[] criteria) where T : new() =>
            criteria.Aggregate(source, (filtered, currentCriteria) => currentCriteria.ApplyTo(filtered, db));
    }
}