using System;
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
        
        public static IEnumerable<T> Tap<T>(this IEnumerable<T> items, Action<T> tap)
        {
            return items.Select(item =>
            {
                tap(item);
                return item;
            });
        }
    }
}