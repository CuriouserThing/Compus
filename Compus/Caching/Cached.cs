using System;
using Compus.Equality;

namespace Compus.Caching
{
    public class Cached<T> : EquatablePart<Cached<T>> where T : IEquatablePart<T>
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

        #region Equality

        protected override IFullEqualityComparer<Cached<T>> EqualityComparer => Comparer;

        // We only care about the universal time in seconds
        private static readonly IFullEqualityComparer<Cached<T>> Comparer = new Identity<Cached<T>>()
            .With(cached => cached.Item)
            .With(cached => (cached.Timestamp.UtcDateTime - DateTime.MinValue).Ticks / TimeSpan.TicksPerSecond)
            .ToComparer();

        #endregion
    }
}
