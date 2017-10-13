using System.Linq;

namespace CHC.Consent.Utils
{
    public static class PagingExtensions
    {
        public static IQueryable<T> GetPage<T>(this IQueryable<T> allTheThings, int pageIndex, int pageSize) =>
            allTheThings.Skip(pageIndex * pageSize).Take(pageSize);
    }
}