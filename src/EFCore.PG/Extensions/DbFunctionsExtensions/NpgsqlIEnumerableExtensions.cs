using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlIEnumerableExtensions
    {
        public static TDest[] ArrayAgg<TDest>(this IEnumerable<TDest> _)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static TDest[] ArrayAgg<TSource, TDest>(this IEnumerable<TSource> _, Func<TSource, TDest> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }

        public static byte BitAnd(this IEnumerable<byte> _)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static short BitAnd(this IEnumerable<short> _)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static int BitAnd(this IEnumerable<int> _)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static long BitAnd(this IEnumerable<long> _)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static byte BitAnd<TSource>(this IEnumerable<TSource> _, Func<TSource, byte> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static short BitAnd<TSource>(this IEnumerable<TSource> _, Func<TSource, short> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static int BitAnd<TSource>(this IEnumerable<TSource> _, Func<TSource, int> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static long BitAnd<TSource>(this IEnumerable<TSource> _, Func<TSource, long> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }

        public static byte BitOr(this IEnumerable<byte> _)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static short BitOr(this IEnumerable<short> _)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static int BitOr(this IEnumerable<int> _)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static long BitOr(this IEnumerable<long> _)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static byte BitOr<TSource>(this IEnumerable<TSource> _, Func<TSource, byte> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static short BitOr<TSource>(this IEnumerable<TSource> _, Func<TSource, short> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static int BitOr<TSource>(this IEnumerable<TSource> _, Func<TSource, int> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static long BitOr<TSource>(this IEnumerable<TSource> _, Func<TSource, long> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }

        public static bool BoolAnd(this IEnumerable<bool> _)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static bool BoolAnd<TSource>(this IEnumerable<TSource> _, Func<TSource, bool> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static bool BoolOr(this IEnumerable<bool> _)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static bool BoolOr<TSource>(this IEnumerable<TSource> _, Func<TSource, bool> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }

        public static TDest[] JsonAgg<TDest>(this IEnumerable<TDest> _)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static TDest[] JsonAgg<TSource, TDest>(this IEnumerable<TSource> _, Func<TSource, TDest> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }

        public static string JsonObjectAgg<TSource, TKey, TValue>(this IEnumerable<TSource> _, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }

        public static string StringAgg(this IEnumerable<string> _, string delimiter)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }
        public static string StringAgg<TSource>(this IEnumerable<TSource> _, string delimiter, Func<TSource, string> selector)
        {
            throw new InvalidOperationException("This method is for use with Entity Framework Core only and has no in-memory implementation.");
        }


    }
}
