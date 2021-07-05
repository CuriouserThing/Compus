using System;

namespace Compus.Caching
{
    public class Cached<T>
    {
        public Cached(T item, DateTimeOffset timestamp)
        {
            Item      = item;
            Timestamp = timestamp;
        }

        public T Item { get; }

        public DateTimeOffset Timestamp { get; }

        public static implicit operator T(Cached<T> cachedItem)
        {
            return cachedItem.Item;
        }
    }
}
