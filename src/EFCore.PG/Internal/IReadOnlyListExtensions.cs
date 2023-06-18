using System.Collections;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

internal static class IReadOnlyListExtensions
{
    public static IReadOnlyList<T> Slice<T>(this IReadOnlyList<T> list, int start)
        => new IReadOnlyListSlice<T>(list, start);

    private sealed class IReadOnlyListSlice<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _underlying;
        private readonly int _start;

        internal IReadOnlyListSlice(IReadOnlyList<T> underlying, int start)
        {
            _underlying = underlying;
            _start = start;
        }

        public IEnumerator<T> GetEnumerator() => _underlying.Skip(_start).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _underlying.Count - _start;

        public T this[int index] => _underlying[_start + index];
    }
}