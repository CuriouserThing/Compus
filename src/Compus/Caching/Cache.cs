using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

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
    ///     Each item requires between 20 and 24 bytes of memory, in addition to the size of <see cref="TItem" /> itself.
    /// </remarks>
    internal class Cache<TKey, TItem> : Cache,
                                        ICollection<Cached<TItem>>,
                                        IDictionary<TKey, Cached<TItem>>
    {
        private readonly DateTimeOffset _epoch;
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly Func<TItem, TKey> _keySelector;
        private readonly object _syncRoot = new();

        private int[] _bucketHeads;
        private int _count;
        private Entry[] _entries;
        private int _head;
        private int _tail;

        public Cache(Func<TItem, TKey>       keySelector,
                     IEqualityComparer<TKey> keyComparer,
                     int                     capacity)
        {
            // Cache timestamps are only precise to the second, so truncate the epoch to the last second.
            DateTimeOffset now = DateTimeOffset.Now;
            long ticks = now.Ticks / TimeSpan.TicksPerSecond * TimeSpan.TicksPerSecond;
            _epoch = new DateTimeOffset(ticks, now.Offset);

            _keySelector = keySelector;
            _keyComparer = keyComparer;
            InitArrays(capacity, out _bucketHeads, out _entries);
            _count = 0;
            ResetHeadAndTail();
        }

        public int Capacity => _entries.Length;

        private static void InitArrays(int cacheSize, out int[] bucketHeads, out Entry[] entries)
        {
            // Trying to test caches with capacity under 3 is a headache, so...don't!
            cacheSize = Math.Max(3, cacheSize);

            int bucketCount = CalculateBucketCount(cacheSize);
            bucketHeads = new int[bucketCount];
            entries     = new Entry[cacheSize];

            Array.Fill(bucketHeads, -1);

            int length = entries.Length;
            for (var i = 0; i < length; i++)
            {
                entries[i] = new Entry
                {
                    Prev       = i - 1,
                    Next       = i + 1,
                    Item       = default(TItem)!,
                    BucketNext = -1,
                    Timestamp  = 0,
                };
            }

            // Tie the queue into a loop
            entries[0].Prev          = length - 1;
            entries[length - 1].Next = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Entry
        {
            /// <summary>default(TItem) while the entry is unused.</summary>
            public TItem Item;

            // We designed this to pack tightly into an array.
            // If TItem is a reference type, Entry will never have padding (assuming x86 or x64).
            // If TItem is a value type, Entry will only have padding if TItem's size is less than 4.
            // Since TItem requires an embedded key, that last case isn't likely.
            // But of course we have no control over what *actually* happens in managed .NET :v
            // Just don't go sticking Entry's Item field in-between the int fields or the compiler will get funny ideas :3c

            /// <summary>Always points to prev node in queue, even while unused.</summary>
            public int Prev;

            /// <summary>Always points to next node in queue, even while unused.</summary>
            public int Next;

            /// <summary>-1 while the entry is unused.</summary>
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

        public IEnumerator<Cached<TItem>> GetEnumerator()
        {
            return new CachedItemEnumerator(this);
        }

        public void Add(Cached<TItem> value)
        {
            lock (_syncRoot)
            {
                TKey key = _keySelector(value.Item);
                ref int link = ref GetIndex(key);
                if (link > -1)
                {
                    throw new ArgumentException($"The key {key} embedded in the value {value} already exists in this cache.", nameof(value));
                }

                Add(ref link, value);
            }
        }

        public bool Remove(Cached<TItem> value)
        {
            lock (_syncRoot)
            {
                TKey key = _keySelector(value.Item);
                ref int link = ref GetIndex(key);
                if (link < 0 || !ItemFromEntry(_entries[link]).Equals(value))
                {
                    return false;
                }

                Remove(ref link);
                return true;
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                InitArrays(Capacity, out _bucketHeads, out _entries);
                _count = 0;
                ResetHeadAndTail();
            }
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

                lock (_syncRoot)
                {
                    ref int link = ref GetIndex(key, value.Item);
                    Add(ref link, value);
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (_syncRoot)
            {
                return GetIndex(key) > -1;
            }
        }

        public bool TryGetValue(TKey key, [NotNullWhen(true)] out Cached<TItem> item)
        {
            if (key is null) { throw new ArgumentNullException(nameof(key)); }

            lock (_syncRoot)
            {
                int index = GetIndex(key);
                if (index > -1)
                {
                    item = ItemFromEntry(_entries[index]);
                    return true;
                }
                else
                {
                    item = default(Cached<TItem>)!;
                    return false;
                }
            }
        }

        public void Add(TKey key, Cached<TItem> value)
        {
            if (key is null) { throw new ArgumentNullException(nameof(key)); }

            lock (_syncRoot)
            {
                ref int link = ref GetIndex(key, value.Item);
                if (link > -1)
                {
                    throw new ArgumentException($"The key {key} already exists in this cache.", nameof(key));
                }

                Add(ref link, value);
            }
        }

        public bool Remove(TKey key)
        {
            if (key is null) { throw new ArgumentNullException(nameof(key)); }

            lock (_syncRoot)
            {
                ref int link = ref GetIndex(key);
                if (link < 0) { return false; }

                Remove(ref link);
                return true;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (_syncRoot)
                {
                    var keys = new TKey[_count];
                    Entry entry = _entries[_head];
                    for (var i = 0; i < _count; i++)
                    {
                        keys[i] = _keySelector(entry.Item);
                        entry   = _entries[entry.Next];
                    }

                    return keys;
                }
            }
        }

        public ICollection<Cached<TItem>> Values
        {
            get
            {
                lock (_syncRoot)
                {
                    var values = new Cached<TItem>[_count];
                    Entry entry = _entries[_head];
                    for (var i = 0; i < _count; i++)
                    {
                        values[i] = ItemFromEntry(entry);
                        entry     = _entries[entry.Next];
                    }

                    return values;
                }
            }
        }

        #endregion

        #region Heavy lifting

        /// <remarks>Not thread-safe.</remarks>
        private ref int GetIndex(TKey key, TItem item)
        {
            if (_keyComparer.Equals(key, _keySelector(item)))
            {
                return ref GetIndex(key);
            }
            else
            {
                throw new ArgumentException($"The given key {key} does not match the key embedded in the given item {item}.", nameof(item));
            }
        }

        /// <remarks>Not thread-safe.</remarks>
        private ref int GetIndex(TKey key)
        {
            int hash = _keyComparer.GetHashCode(key!);
            int bucketIndex = (hash & int.MaxValue) % _bucketHeads.Length;
            ref int index = ref _bucketHeads[bucketIndex];
            while (index != -1)
            {
                TKey cacheKey = _keySelector(_entries[index].Item);
                if (_keyComparer.Equals(key, cacheKey))
                {
                    return ref index;
                }

                index = ref _entries[index].BucketNext;
            }

            return ref index;
        }

        /// <remarks>Not thread-safe.</remarks>
        private void Add(ref int link, Cached<TItem> value)
        {
            if (link < 0)
            {
                // This is a new key, so we need to requisition the entry after the queue tail.
                // If we're at capacity, this means evicting the oldest item. Advance the head to the second-oldest item and we'll overwrite the old item's values below.
                // Otherwise, no eviction is necessary so we'll just increment the count.
                if (_count == _entries.Length)
                {
                    _head = _entries[_head].Next;
                }
                else
                {
                    _count++;
                }

                // Link our requisitioned entry to the previous bucket list node...
                link = _entries[_tail].Next;

                // ...then terminate the bucket list at this entry.
                _entries[link].BucketNext = -1;
            }

            int index = link;
            int timestamp = ConvertTimestamp(value.Timestamp);
            _entries[index].Item      = value.Item;
            _entries[index].Timestamp = timestamp;

            if (_count == 1)
            {
                _head = index;
                _tail = index;
            }
            else
            {
                RemoveEntryFromQueueLoop(index);

                int p = _tail;
                int n = _entries[p].Next;

                // If timestamps are in agreement with insertion order, this will always be false and we'll always insert items at the end of the queue.
                while (timestamp < _entries[p].Timestamp && n != _head)
                {
                    n = p;
                    p = _entries[p].Prev;
                }

                if (p      == _tail) { _tail = index; }
                else if (n == _head) { _head = index; }

                InsertEntryIntoQueueLoop(index, p, n);
            }
        }

        /// <remarks>Not thread-safe.</remarks>
        private void Remove(ref int link)
        {
            int index = link;

            _count--;
            if (_count == 0)
            {
                ResetHeadAndTail();
            }
            else
            {
                RemoveEntryFromQueueLoop(index);
                InsertEntryIntoQueueLoop(index, _entries[_head].Prev, _head);
            }

            // Remove linked entry from its bucket list, re-linking the list together (or "deleting" the list).
            link = _entries[index].BucketNext;

            // Clean the remaining entry fields.
            _entries[index].Item       = default(TItem)!;
            _entries[index].BucketNext = -1;
            _entries[index].Timestamp  = 0;
        }

        /// <remarks>Not thread-safe.</remarks>
        private void RemoveEntryFromQueueLoop(int index)
        {
            int prev = _entries[index].Prev;
            int next = _entries[index].Next;

            // Bump the head or tail if needed.
            if (index      == _head) { _head = next; }
            else if (index == _tail) { _tail = prev; }

            // Remove entry from its current queue position, re-linking the queue together.
            _entries[prev].Next = next;
            _entries[next].Prev = prev;
        }

        /// <remarks>Not thread-safe.</remarks>
        private void InsertEntryIntoQueueLoop(int index, int prev, int next)
        {
            _entries[index].Prev = prev;
            _entries[index].Next = next;
            _entries[prev].Next  = index;
            _entries[next].Prev  = index;
        }

        /// <remarks>Not thread-safe.</remarks>
        private void ResetHeadAndTail()
        {
            _head = 0;
            _tail = _entries[_head].Prev;
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

        private Cached<TItem> ItemFromEntry(Entry entry)
        {
            return new(entry.Item, ConvertTimestamp(entry.Timestamp));
        }

        #endregion

        #region Explicit ICollection implementation

        private bool Contains(KeyValuePair<TKey, Cached<TItem>> kvp)
        {
            (TKey key, Cached<TItem> otherValue) = kvp;
            return TryGetValue(key, out Cached<TItem> thisValue) &&
                   thisValue.Equals(otherValue);
        }

        bool ICollection<Cached<TItem>>.Contains(Cached<TItem> value)
        {
            TKey key = _keySelector(value.Item);
            return TryGetValue(key, out Cached<TItem> thisValue) &&
                   thisValue.Equals(value);
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

        void ICollection<Cached<TItem>>.CopyTo(Cached<TItem>[] array, int arrayIndex)
        {
            if (array is null) { throw new ArgumentNullException(nameof(array)); }

            lock (_syncRoot)
            {
                Span<Cached<TItem>> span = GetDestinationSpan(array, arrayIndex);
                Entry entry = _entries[_head];
                for (var i = 0; i < _count; i++)
                {
                    span[i] = ItemFromEntry(entry);
                    entry   = _entries[entry.Next];
                }
            }
        }

        void ICollection<KeyValuePair<TKey, Cached<TItem>>>.CopyTo(KeyValuePair<TKey, Cached<TItem>>[] array, int arrayIndex)
        {
            if (array is null) { throw new ArgumentNullException(nameof(array)); }

            lock (_syncRoot)
            {
                Span<KeyValuePair<TKey, Cached<TItem>>> span = GetDestinationSpan(array, arrayIndex);
                Entry entry = _entries[_head];
                for (var i = 0; i < _count; i++)
                {
                    TKey key = _keySelector(entry.Item);
                    Cached<TItem> value = ItemFromEntry(entry);
                    span[i] = new KeyValuePair<TKey, Cached<TItem>>(key, value);
                    entry   = _entries[entry.Next];
                }
            }
        }

        bool ICollection<Cached<TItem>>.IsReadOnly => false;

        bool ICollection<KeyValuePair<TKey, Cached<TItem>>>.IsReadOnly => false;

        #endregion

        #region Explicit IEnumerable implementation

        IEnumerator<KeyValuePair<TKey, Cached<TItem>>> IEnumerable<KeyValuePair<TKey, Cached<TItem>>>.GetEnumerator()
        {
            return new KeyValuePairEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private abstract class Enumerator<T> : IEnumerator<T> where T : notnull
        {
            protected readonly Cache<TKey, TItem> Cache;

            private int _position;

            protected Enumerator(Cache<TKey, TItem> cache)
            {
                Cache = cache;
                Reset();
            }

            public void Reset()
            {
                lock (Cache._syncRoot)
                {
                    _position = Cache._entries[Cache._head].Prev;
                }
            }

            public bool MoveNext()
            {
                lock (Cache._syncRoot)
                {
                    bool passedEnd = Cache._count == 0 || _position == Cache._tail;
                    _position = Cache._entries[_position].Next;
                    return !passedEnd;
                }
            }

            public T Current
            {
                get
                {
                    lock (Cache._syncRoot)
                    {
                        Entry entry = Cache._entries[_position];
                        return SelectFromEntry(entry);
                    }
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            protected abstract T SelectFromEntry(Entry entry);
        }

        private class CachedItemEnumerator : Enumerator<Cached<TItem>>
        {
            public CachedItemEnumerator(Cache<TKey, TItem> cache) : base(cache)
            {
            }

            protected override Cached<TItem> SelectFromEntry(Entry entry)
            {
                return Cache.ItemFromEntry(entry);
            }
        }

        private class KeyValuePairEnumerator : Enumerator<KeyValuePair<TKey, Cached<TItem>>>
        {
            public KeyValuePairEnumerator(Cache<TKey, TItem> cache) : base(cache)
            {
            }

            protected override KeyValuePair<TKey, Cached<TItem>> SelectFromEntry(Entry entry)
            {
                TKey key = Cache._keySelector(entry.Item);
                Cached<TItem> value = Cache.ItemFromEntry(entry);
                return new KeyValuePair<TKey, Cached<TItem>>(key, value);
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
