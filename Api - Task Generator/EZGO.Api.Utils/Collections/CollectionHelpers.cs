using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.Collections
{
    public static class CollectionHelpers
    {
        public static IEnumerable<T> Flatten<T, R>(this IEnumerable<T> source, Func<T, R> recursion) where R : IEnumerable<T>
        {
            return source != null ? source.SelectMany(x => (x != null && recursion != null && recursion(x) != null && recursion(x).Any()) ? recursion(x).Flatten(recursion) : null).Where(x => x != null) : null;
        }
    }
}
