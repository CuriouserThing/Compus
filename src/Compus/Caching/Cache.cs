using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Compus.Caching
{
    /// <remarks>
    ///     The following are O(1) or O(1)+ (amortized to constant) operations:
    ///     <list>
    ///         <item>
    ///             <description>Accessing any item by key</description>
    ///         </item>
    ///         <item>
    ///             <description>Accessing the oldest item</description>
    ///         </item>
    ///         <item>
    ///             <description>Accessing the newest item</description>
    ///         </item>
    ///         <item>
    ///             <description>Adding a new item containing a new key, evicting the oldest item if at capacity</description>
    ///         </item>
    ///         <item>
    ///             <description>Replacing an old item with a new item containing the same key</description>
    ///         </item>
    ///         <item>
    ///             <description>Refreshing an old item to make it newest</description>
    ///         </item>
    ///         <item>
    ///             <description>Removing an item</description>
    ///         </item>
    ///     </list>
    ///     Each item requires between 20 and 24 bytes of memory, in addition to the sizes of <see cref="TKey" /> and
    ///     <see cref="TItem" /> themselves.
    /// </remarks>
    internal class Cache<TKey, TItem> : Cache, IDictionary<TKey, Cached<TItem>>
    {
        private const int DefaultInitialCapacity = 16;
        private const float GrowthFactor = 2.0f;

        private readonly DateTimeOffset _epoch;
        private readonly IEvictionPolicy _evictionPolicy;
        private readonly IEqualityComparer<TKey> _keyComparer;

        private int[] _bucketHeads;
        private int _count;
        private Entry[] _entries;
        private TItem[] _items;
        private TKey[] _keys;
        private int _tail;

        public Cache(IEqualityComparer<TKey> keyComparer, IEvictionPolicy evictionPolicy, int? initialCapacity = null)
        {
            // Cache timestamps are only precise to the second, so truncate the epoch to the last second.
            DateTimeOffset now = DateTimeOffset.Now;
            long ticks = now.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond;
            _epoch = new DateTimeOffset(ticks, now.Offset);

            _keyComparer    = keyComparer;
            _evictionPolicy = evictionPolicy;
            int capacity = initialCapacity ?? DefaultInitialCapacity;
            InitArrays(capacity, out _bucketHeads, out _keys, out _items, out _entries);
        }

        public int Capacity => _entries.Length - 2;

        private static void InitArrays(int         capacity,
                                       out int[]   bucketHeads,
                                       out TKey[]  keys,
                                       out TItem[] items,
                                       out Entry[] entries)
        {
            // Trying to test caches with capacity under 3 is a headache, so...don't!
            capacity = Math.Max(3, capacity);

            int bucketCount = CalculateBucketCount(capacity);
            bucketHeads = new int[bucketCount];
            keys        = new TKey[capacity  + 1];
            items       = new TItem[capacity + 1];
            entries     = new Entry[capacity + 2];

            for (var i = 0; i < entries.Length; i++)
            {
                entries[i] = new Entry
                {
                    Prev = i - 1,
                    Next = i + 1,
                };
            }
        }

        private void Grow(int capacity)
        {
            Rehash(capacity);
            int start = _entries.Length;

            var newKeys = new TKey[capacity     + 1];
            var newItems = new TItem[capacity   + 1];
            var newEntries = new Entry[capacity + 2];

            Array.Copy(_keys,    newKeys,    _keys.Length);
            Array.Copy(_items,   newItems,   _items.Length);
            Array.Copy(_entries, newEntries, _entries.Length);

            _keys    = newKeys;
            _items   = newItems;
            _entries = newEntries;

            for (int i = start; i < newEntries.Length; i++)
            {
                newEntries[i] = new Entry
                {
                    Prev = i - 1,
                    Next = i + 1,
                };
            }
        }

        private void Rehash(int capacity)
        {
            int bucketCount = CalculateBucketCount(capacity);
            _bucketHeads = new int[bucketCount];

            var index = 0;
            while (index != _tail)
            {
                index = _entries[index].Next;
                TKey key = _keys[index];
                int hash = _keyComparer.GetHashCode(key!);
                int bucketIndex = (hash & int.MaxValue) % bucketCount;
                ref int link = ref _bucketHeads[bucketIndex];
                while (link != 0)
                {
                    link = ref _entries[link].BucketNext;
                }

                link = index;
            }
        }

        private struct Entry
        {
            /// <summary>Always points to prev node in queue, even while unused.</summary>
            public int Prev;

            /// <summary>Always points to next node in queue, even while unused.</summary>
            public int Next;

            /// <summary>0 while the entry is unused.</summary>
            public int BucketNext;

            /// <summary>
            ///     Seconds since the cache epoch (or before, in case of server time oddities).
            ///     0 while the entry is unused.
            /// </summary>
            public int Timestamp;
        }

        #region Public interface implementation

        // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
        public int Count => _count;

        public void Clear()
        {
            InitArrays(Capacity, out _bucketHeads, out _keys, out _items, out _entries);
            _count = 0;
            _tail  = 0;
        }

        public Cached<TItem> this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out Cached<TItem> value))
                {
                    return value;
                }
                else
                {
                    throw new KeyNotFoundException($"The key {key} was not found in the cache.");
                }
            }
            set
            {
                if (key is null) { throw new ArgumentNullException(nameof(key)); }

                ref int link = ref GetIndex(key);
                Add(ref link, key, value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return GetIndex(key) != 0;
        }

        public bool TryGetValue(TKey key, [NotNullWhen(true)] out Cached<TItem> item)
        {
            if (key is null) { throw new ArgumentNullException(nameof(key)); }

            int index = GetIndex(key);
            if (index != 0)
            {
                item = ValueFromIndex(index);
                return true;
            }
            else
            {
                item = default(Cached<TItem>)!;
                return false;
            }
        }

        public void Add(TKey key, Cached<TItem> value)
        {
            if (key is null) { throw new ArgumentNullException(nameof(key)); }

            ref int link = ref GetIndex(key);
            if (link != 0)
            {
                throw new ArgumentException($"The key {key} already exists in this cache.", nameof(key));
            }

            Add(ref link, key, value);
        }

        public bool Remove(TKey key)
        {
            if (key is null) { throw new ArgumentNullException(nameof(key)); }

            ref int link = ref GetIndex(key);
            if (link == 0) { return false; }

            Remove(ref link);
            return true;
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var keys = new TKey[_count];
                int index = _entries[0].Next;
                for (var i = 0; i < _count; i++)
                {
                    keys[i] = _keys[index];
                    index   = _entries[index].Next;
                }

                return keys;
            }
        }

        public ICollection<Cached<TItem>> Values
        {
            get
            {
                var values = new Cached<TItem>[_count];
                int index = _entries[0].Next;
                for (var i = 0; i < _count; i++)
                {
                    values[i] = ValueFromIndex(index);
                    index     = _entries[index].Next;
                }

                return values;
            }
        }

        #endregion

        #region Heavy lifting

        private ref int GetIndex(TKey key)
        {
            int hash = _keyComparer.GetHashCode(key!);
            int bucketIndex = (hash & int.MaxValue) % _bucketHeads.Length;
            ref int link = ref _bucketHeads[bucketIndex];
            while (link != 0)
            {
                TKey cacheKey = _keys[link];
                if (_keyComparer.Equals(key, cacheKey))
                {
                    return ref link;
                }

                link = ref _entries[link].BucketNext;
            }

            return ref link;
        }

        private void Add(ref int link, TKey key, Cached<TItem> value)
        {
            if (link == 0)
            {
                if (_count == Capacity)
                {
                    int oldest = _entries[0].Next;
                    TimeSpan lifespan = DateTimeOffset.Now - ConvertTimestamp(_entries[oldest].Timestamp);
                    if (_evictionPolicy.CanEvictOldest(lifespan, _count))
                    {
                        TKey oldestKey = _keys[oldest];
                        ref int oldestLink = ref GetIndex(oldestKey);
                        Remove(ref oldestLink);
                    }
                    else
                    {
                        Grow((int)(_count * GrowthFactor));
                    }
                }

                _count++;
                link = _entries[_tail].Next;
            }

            int index = link;
            int timestamp = ConvertTimestamp(value.Timestamp);
            _keys[index]              = key;
            _items[index]             = value.Item;
            _entries[index].Timestamp = timestamp;

            if (_tail == index)
            {
                _tail = _entries[index].Prev;
            }

            RemoveEntryFromQueue(index);
            int p = _tail;
            int n = _entries[p].Next;

            // If timestamps are in agreement with insertion order, this will always be false and we'll always insert items at the end of the queue.
            while (p != 0 && timestamp < _entries[p].Timestamp)
            {
                n = p;
                p = _entries[p].Prev;
            }

            InsertEntryIntoQueue(index, p, n);
            if (_tail == p)
            {
                _tail = index;
            }
        }

        private void Remove(ref int link)
        {
            int index = link;

            _count--;
            if (_tail == index)
            {
                _tail = _entries[index].Prev;
            }
            else
            {
                RemoveEntryFromQueue(index);
                InsertEntryIntoQueue(index, _tail, _entries[_tail].Next);
            }

            // Remove linked entry from its bucket list, re-linking the list together (or "deleting" the list).
            link = _entries[index].BucketNext;

            // Clean the remaining fields.
            _keys[index]               = default(TKey)!;
            _items[index]              = default(TItem)!;
            _entries[index].BucketNext = default(int);
            _entries[index].Timestamp  = default(int);
        }

        private void RemoveEntryFromQueue(int index)
        {
            int prev = _entries[index].Prev;
            int next = _entries[index].Next;
            _entries[prev].Next = next;
            _entries[next].Prev = prev;
        }

        private void InsertEntryIntoQueue(int index, int prev, int next)
        {
            _entries[index].Prev = prev;
            _entries[index].Next = next;
            _entries[prev].Next  = index;
            _entries[next].Prev  = index;
        }

        #endregion

        #region Helpers

        private DateTimeOffset ConvertTimestamp(int timestamp)
        {
            return _epoch.AddSeconds(timestamp);
        }

        private int ConvertTimestamp(DateTimeOffset timestamp)
        {
            return (int)(timestamp - _epoch).TotalSeconds;
        }

        private Cached<TItem> ValueFromIndex(int index)
        {
            DateTimeOffset timestamp = ConvertTimestamp(_entries[index].Timestamp);
            return new Cached<TItem>(_items[index], timestamp);
        }

        #endregion

        #region Explicit ICollection implementation

        private bool Contains(KeyValuePair<TKey, Cached<TItem>> kvp)
        {
            (TKey key, Cached<TItem> otherValue) = kvp;
            return TryGetValue(key, out Cached<TItem> thisValue) &&
                   thisValue.Equals(otherValue);
        }

        bool ICollection<KeyValuePair<TKey, Cached<TItem>>>.Contains(KeyValuePair<TKey, Cached<TItem>> kvp)
        {
            return Contains(kvp);
        }

        void ICollection<KeyValuePair<TKey, Cached<TItem>>>.Add(KeyValuePair<TKey, Cached<TItem>> kvp)
        {
            Add(kvp.Key, kvp.Value);
        }

        bool ICollection<KeyValuePair<TKey, Cached<TItem>>>.Remove(KeyValuePair<TKey, Cached<TItem>> kvp)
        {
            return Contains(kvp) && Remove(kvp.Key);
        }

        private Span<T> GetDestinationSpan<T>(T[] array, int arrayIndex)
        {
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            int space = array.Length - arrayIndex;
            if (space < _count)
            {
                throw new ArgumentException($"Cache has a count of {_count}, which is greater than {space}.", nameof(array));
            }

            return new Span<T>(array, arrayIndex, _count);
        }

        void ICollection<KeyValuePair<TKey, Cached<TItem>>>.CopyTo(KeyValuePair<TKey, Cached<TItem>>[] array, int arrayIndex)
        {
            if (array is null) { throw new ArgumentNullException(nameof(array)); }

            Span<KeyValuePair<TKey, Cached<TItem>>> span = GetDestinationSpan(array, arrayIndex);
            Entry entry = _entries[_entries[0].Next];
            int index = _entries[0].Next;
            for (var i = 0; i < _count; i++)
            {
                TKey key = _keys[index];
                Cached<TItem> value = ValueFromIndex(index);
                span[i] = new KeyValuePair<TKey, Cached<TItem>>(key, value);
                index   = entry.Next;
            }
        }

        bool ICollection<KeyValuePair<TKey, Cached<TItem>>>.IsReadOnly => false;

        #endregion

        #region Explicit IEnumerable implementation

        IEnumerator<KeyValuePair<TKey, Cached<TItem>>> IEnumerable<KeyValuePair<TKey, Cached<TItem>>>.GetEnumerator()
        {
            return new KeyValuePairEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new KeyValuePairEnumerator(this);
        }

        private abstract class Enumerator<T> : IEnumerator<T> where T : notnull
        {
            private bool _passedEnd;

            protected Enumerator(Cache<TKey, TItem> cache)
            {
                Cache = cache;
            }

            protected Cache<TKey, TItem> Cache { get; }

            protected int Position { get; private set; }

            public void Reset()
            {
                Position = 0;
            }

            public bool MoveNext()
            {
                if (_passedEnd) { return false; }

                _passedEnd = Position == Cache._tail;
                Position   = Cache._entries[Position].Next;
                return !_passedEnd;
            }

            public abstract T Current { get; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }

        private class KeyValuePairEnumerator : Enumerator<KeyValuePair<TKey, Cached<TItem>>>
        {
            public KeyValuePairEnumerator(Cache<TKey, TItem> cache) : base(cache)
            {
            }

            public override KeyValuePair<TKey, Cached<TItem>> Current
            {
                get
                {
                    TKey key = Cache._keys[Position];
                    Cached<TItem> value = Cache.ValueFromIndex(Position);
                    return new KeyValuePair<TKey, Cached<TItem>>(key, value);
                }
            }
        }

        #endregion
    }

    internal abstract class Cache
    {
        private static readonly int[] BertrandPrimes =
        {
            /*2, 3,*/ 5, 7, 13, 23, 43, 83,
            163, 317, 631, 1259, 2503, 5003, 9973, 19937,
            39869, 79699, 159389, 318751, 637499, 1274989, 2549951, 5099893,
            10199767, 20399531, 40799041, 81598067, 163196129, 326392249, 652784471, 1305568919,
        };

        protected static int CalculateBucketCount(int cacheSize)
        {
            foreach (int p in BertrandPrimes)
            {
                if (p > cacheSize)
                {
                    // This puts the load factor for a full cache somewhere between 0.5 and 1.0
                    return p;
                }
            }

            return int.MaxValue; // coincidentally a Mersenne prime!
        }
    }
}
