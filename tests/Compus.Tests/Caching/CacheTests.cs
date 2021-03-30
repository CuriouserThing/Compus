using System;
using Compus.Equality;
using Compus.Resources;
using Xunit;

namespace Compus.Caching
{
    public class CacheTests
    {
        private static readonly Snowflake KeyA = 272497508301668362;
        private static readonly Snowflake KeyB = 246863212597739531;
        private static readonly Snowflake KeyC = 717727937159233577;

        private static readonly Item ItemA1 = new(KeyA, "Empress");
        private static readonly Item ItemA2 = new(KeyA, "Cutiefly");
        private static readonly Item ItemB1 = new(KeyB, "Sweetpea");
        private static readonly Item ItemC1 = new(KeyC, "Slider");

        private static readonly Cached<Item> ItemA1Time1 = new(ItemA1, new DateTimeOffset(2020, 5, 1, 12, 30, 55, TimeSpan.Zero));
        private static readonly Cached<Item> ItemB1Time2 = new(ItemB1, ItemA1Time1.Timestamp + TimeSpan.FromMinutes(7223956));
        private static readonly Cached<Item> ItemC1Time3 = new(ItemC1, ItemB1Time2.Timestamp + TimeSpan.FromMinutes(37));
        private static readonly Cached<Item> ItemA2Time4 = new(ItemA2, ItemC1Time3.Timestamp + TimeSpan.FromMinutes(1001));

        private static Cache<Snowflake, Item> CreateCache()
        {
            return new(item => item.Key, Snowflake.EqualityComparer, 100);
        }

        [Fact]
        public void EmptyCache_Add_CountIs1()
        {
            Cache<Snowflake, Item> cache = CreateCache();

            cache.Add(ItemA1Time1);

            Assert.Single(cache);
        }

        [Fact]
        public void Count1Cache_Remove_EmptyCache()
        {
            Cache<Snowflake, Item> cache = CreateCache();
            cache.Add(ItemA1Time1);

            cache.Remove(ItemA1Time1);

            Assert.Empty(cache);
        }

        [Fact]
        public void Count1Cache_Add_Count2Cache()
        {
            Cache<Snowflake, Item> cache = CreateCache();
            cache.Add(ItemA1Time1);

            cache.Add(ItemB1Time2);

            Assert.Equal(2, cache.Count);
        }

        [Fact]
        public void Count1Cache_AddLaterItem_OrderedCache()
        {
            Cache<Snowflake, Item> cache = CreateCache();
            cache.Add(ItemA1Time1);

            cache.Add(ItemB1Time2);

            Assert.Collection(cache, new Action<Cached<Item>>[]
            {
                cached => Assert.Equal(ItemA1Time1.Item.Key, cached.Item.Key),
                cached => Assert.Equal(ItemB1Time2.Item.Key, cached.Item.Key),
            });
        }

        [Fact]
        public void Count1Cache_AddEarlierItem_OrderedCache()
        {
            Cache<Snowflake, Item> cache = CreateCache();
            cache.Add(ItemB1Time2);

            cache.Add(ItemA1Time1);

            Assert.Collection(cache, new Action<Cached<Item>>[]
            {
                cached => Assert.Equal(ItemA1Time1.Item.Key, cached.Item.Key),
                cached => Assert.Equal(ItemB1Time2.Item.Key, cached.Item.Key),
            });
        }

        [Fact]
        public void Count2Cache_Remove_Count1Cache()
        {
            Cache<Snowflake, Item> cache = CreateCache();
            cache.Add(ItemA1Time1);
            cache.Add(ItemB1Time2);

            cache.Remove(ItemA1Time1);

            Assert.Single(cache);
        }

        [Fact]
        public void Count2Cache_Add_Count3Cache()
        {
            Cache<Snowflake, Item> cache = CreateCache();
            cache.Add(ItemA1Time1);
            cache.Add(ItemB1Time2);

            cache.Add(ItemC1Time3);

            Assert.Equal(3, cache.Count);
        }

        [Fact]
        public void Count3Cache_AddLatestItem_OrderedCache()
        {
            Cache<Snowflake, Item> cache = CreateCache();
            cache.Add(ItemA1Time1);
            cache.Add(ItemB1Time2);

            cache.Add(ItemC1Time3);

            Assert.Collection(cache, new Action<Cached<Item>>[]
            {
                cached => Assert.Equal(ItemA1Time1.Item.Key, cached.Item.Key),
                cached => Assert.Equal(ItemB1Time2.Item.Key, cached.Item.Key),
                cached => Assert.Equal(ItemC1Time3.Item.Key, cached.Item.Key),
            });
        }

        [Fact]
        public void Count3Cache_AddMiddleItem_OrderedCache()
        {
            Cache<Snowflake, Item> cache = CreateCache();
            cache.Add(ItemA1Time1);
            cache.Add(ItemC1Time3);

            cache.Add(ItemB1Time2);

            Assert.Collection(cache, new Action<Cached<Item>>[]
            {
                cached => Assert.Equal(ItemA1Time1.Item.Key, cached.Item.Key),
                cached => Assert.Equal(ItemB1Time2.Item.Key, cached.Item.Key),
                cached => Assert.Equal(ItemC1Time3.Item.Key, cached.Item.Key),
            });
        }

        [Fact]
        public void Count3Cache_AddEarliestItem_OrderedCache()
        {
            Cache<Snowflake, Item> cache = CreateCache();
            cache.Add(ItemB1Time2);
            cache.Add(ItemC1Time3);

            cache.Add(ItemA1Time1);

            Assert.Collection(cache, new Action<Cached<Item>>[]
            {
                cached => Assert.Equal(ItemA1Time1.Item.Key, cached.Item.Key),
                cached => Assert.Equal(ItemB1Time2.Item.Key, cached.Item.Key),
                cached => Assert.Equal(ItemC1Time3.Item.Key, cached.Item.Key),
            });
        }

        [Fact]
        public void Count1Cache_AddNewItemWithSameKey_ItemReplaced()
        {
            Cache<Snowflake, Item> cache = CreateCache();
            cache.Add(ItemA1Time1);

            cache[ItemA1Time1.Item.Key] = ItemA2Time4;

            Assert.Collection(cache, new Action<Cached<Item>>[]
            {
                cached => Assert.Equal(ItemA2Time4.Item.Value, cached.Item.Value),
            });
        }

        [Fact]
        public void Count3Cache_AddLatestItemWithSameKeyAsEarliest_OrderedCache()
        {
            Cache<Snowflake, Item> cache = CreateCache();
            cache.Add(ItemA1Time1);
            cache.Add(ItemB1Time2);
            cache.Add(ItemC1Time3);

            cache[ItemA1Time1.Item.Key] = ItemA2Time4;

            Assert.Collection(cache, new Action<Cached<Item>>[]
            {
                cached => Assert.Equal(ItemB1Time2.Item.Value, cached.Item.Value),
                cached => Assert.Equal(ItemC1Time3.Item.Value, cached.Item.Value),
                cached => Assert.Equal(ItemA2Time4.Item.Value, cached.Item.Value),
            });
        }

        private class Item : EquatablePart<Item>
        {
            public Item(Snowflake key, string? value)
            {
                Key   = key;
                Value = value;
            }

            public Snowflake Key { get; }

            public string? Value { get; }

            #region Equality

            protected sealed override IFullEqualityComparer<Item> EqualityComparer => Comparer;

            private static readonly IFullEqualityComparer<Item> Comparer = new Identity<Item>()
                .With(item => item.Key)
                .With(item => item.Value)
                .ToComparer();

            #endregion
        }
    }
}
