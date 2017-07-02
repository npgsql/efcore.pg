using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Utilities
{
    public static class CollectionExtensions
    {
        public static Queue<T> Enqueue<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
            }

            return queue;
        }
    }
}
