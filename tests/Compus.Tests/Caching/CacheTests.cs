using System;
using Xunit;

namespace Compus.Caching
{
    public class CacheTests
    {
        private static readonly Snowflake KeyA = 272497508301668362;
        private static readonly Snowflake KeyB = 246863212597739531;
        private static readonly Snowflake KeyC = 717727937159233577;

        private const string Item1 = "Empress";
        private const string Item2 = "Cutiefly";
        private const string Item3 = "Sweetpea";
        private const string Item4 = "Slider";

        private static readonly Cached<string> Item1Time1 = new(Item1, new DateTimeOffset(2020, 5, 1, 12, 30, 55, TimeSpan.Zero));
        private static readonly Cached<string> Item3Time2 = new(Item3, Item1Time1.Timestamp + TimeSpan.FromMinutes(7223956));
        private static readonly Cached<string> Item4Time3 = new(Item4, Item3Time2.Timestamp + TimeSpan.FromMinutes(37));
        private static readonly Cached<string> Item2Time4 = new(Item2, Item4Time3.Timestamp + TimeSpan.FromMinutes(1001));

        private static Cache<Snowflake, string> CreateCache()
        {
            return new(Snowflake.EqualityComparer, new NoEvictionPolicy());
        }

        [Fact]
        public void EmptyCache_Add_CountIs1()
        {
            var cache = CreateCache();

            cache.Add(KeyA, Item1Time1);

            Assert.Single(cache);
        }

        [Fact]
        public void Count1Cache_Remove_EmptyCache()
        {
            var cache = CreateCache();
            cache.Add(KeyA, Item1Time1);

            cache.Remove(KeyA);

            Assert.Empty(cache);
        }

        [Fact]
        public void Count1Cache_Add_Count2Cache()
        {
            var cache = CreateCache();
            cache.Add(KeyA, Item1Time1);

            cache.Add(KeyB, Item3Time2);

            Assert.Equal(2, cache.Count);
        }

        [Fact]
        public void Count1Cache_AddLaterItem_OrderedCache()
        {
            var cache = CreateCache();
            cache.Add(KeyA, Item1Time1);

            cache.Add(KeyB, Item3Time2);

            Assert.Collection(cache.Keys, new Action<Snowflake>[]
            {
                key => Assert.Equal(KeyA, key),
                key => Assert.Equal(KeyB, key),
            });
        }

        [Fact]
        public void Count1Cache_AddEarlierItem_OrderedCache()
        {
            var cache = CreateCache();
            cache.Add(KeyB, Item3Time2);

            cache.Add(KeyA, Item1Time1);

            Assert.Collection(cache.Keys, new Action<Snowflake>[]
            {
                key => Assert.Equal(KeyA, key),
                key => Assert.Equal(KeyB, key),
            });
        }

        [Fact]
        public void Count2Cache_Remove_Count1Cache()
        {
            var cache = CreateCache();
            cache.Add(KeyA, Item1Time1);
            cache.Add(KeyB, Item3Time2);

            cache.Remove(KeyA);

            Assert.Single(cache);
        }

        [Fact]
        public void Count2Cache_Add_Count3Cache()
        {
            var cache = CreateCache();
            cache.Add(KeyA, Item1Time1);
            cache.Add(KeyB, Item3Time2);

            cache.Add(KeyC, Item4Time3);

            Assert.Equal(3, cache.Count);
        }

        [Fact]
        public void Count3Cache_AddLatestItem_OrderedCache()
        {
            var cache = CreateCache();
            cache.Add(KeyA, Item1Time1);
            cache.Add(KeyB, Item3Time2);

            cache.Add(KeyC, Item4Time3);

            Assert.Collection(cache.Keys, new Action<Snowflake>[]
            {
                key => Assert.Equal(KeyA, key),
                key => Assert.Equal(KeyB, key),
                key => Assert.Equal(KeyC, key),
            });
        }

        [Fact]
        public void Count3Cache_AddMiddleItem_OrderedCache()
        {
            var cache = CreateCache();
            cache.Add(KeyA, Item1Time1);
            cache.Add(KeyC, Item4Time3);

            cache.Add(KeyB, Item3Time2);

            Assert.Collection(cache.Keys, new Action<Snowflake>[]
            {
                key => Assert.Equal(KeyA, key),
                key => Assert.Equal(KeyB, key),
                key => Assert.Equal(KeyC, key),
            });
        }

        [Fact]
        public void Count3Cache_AddEarliestItem_OrderedCache()
        {
            var cache = CreateCache();
            cache.Add(KeyB, Item3Time2);
            cache.Add(KeyC, Item4Time3);

            cache.Add(KeyA, Item1Time1);

            Assert.Collection(cache.Keys, new Action<Snowflake>[]
            {
                key => Assert.Equal(KeyA, key),
                key => Assert.Equal(KeyB, key),
                key => Assert.Equal(KeyC, key),
            });
        }

        [Fact]
        public void Count1Cache_AddNewItemWithSameKey_ItemReplaced()
        {
            var cache = CreateCache();
            cache.Add(KeyA, Item1Time1);

            cache[KeyA] = Item2Time4;

            Assert.Collection(cache.Values, new Action<Cached<string>>[]
            {
                cached => Assert.Equal(Item2, cached.Item),
            });
        }

        [Fact]
        public void Count3Cache_AddLatestItemWithSameKeyAsEarliest_OrderedCache()
        {
            var cache = CreateCache();
            cache.Add(KeyA, Item1Time1);
            cache.Add(KeyB, Item3Time2);
            cache.Add(KeyC, Item4Time3);

            cache[KeyA] = Item2Time4;

            Assert.Collection(cache.Values, new Action<Cached<string>>[]
            {
                cached => Assert.Equal(Item3, cached.Item),
                cached => Assert.Equal(Item4, cached.Item),
                cached => Assert.Equal(Item2, cached.Item),
            });
        }
    }
}
