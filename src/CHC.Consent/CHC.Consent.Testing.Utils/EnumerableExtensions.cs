using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CHC.Consent.Testing.Utils
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(int index, T item)> AddIndex<T>(this IEnumerable<T> items)
        {
            return items.Select((item, index) => (index, item));
        }
    }
}