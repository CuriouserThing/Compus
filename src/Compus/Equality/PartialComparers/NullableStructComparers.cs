using System;

namespace Compus.Equality.PartialComparers
{
    internal static class NullableStructComparers<TItem> where TItem : notnull
    {
        public abstract class Base<TPart> : NullableComparerBase<TItem, TPart?>
            where TPart : struct, IEquatable<TPart>
        {
            protected Base(Func<TItem, TPart?> selectPart) : base(selectPart)
            {
            }

            protected sealed override bool PartEquals(TPart? x, TPart? y)
            {
                if (x.HasValue)
                {
                    return y.HasValue && x.Value.Equals(y.Value);
                }
                else
                {
                    return !y.HasValue;
                }
            }
        }

        #region Implementations

        public class Boolean : Base<bool>
        {
            public Boolean(Func<TItem, bool?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, bool? obj)
            {
                return hasher.Hash(seed, obj);
            }
        }

        public class Byte : Base<byte>
        {
            public Byte(Func<TItem, byte?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, byte? obj)
            {
                return hasher.Hash(seed, obj);
            }
        }

        public class SByte : Base<sbyte>
        {
            public SByte(Func<TItem, sbyte?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, sbyte? obj)
            {
                return hasher.Hash(seed, obj);
            }
        }

        public class Char : Base<char>
        {
            public Char(Func<TItem, char?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, char? obj)
            {
                return hasher.Hash(seed, obj);
            }
        }

        public class Double : Base<double>
        {
            public Double(Func<TItem, double?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, double? obj)
            {
                return hasher.Hash(seed, obj);
            }
        }

        public class Single : Base<float>
        {
            public Single(Func<TItem, float?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, float? obj)
            {
                return hasher.Hash(seed, obj);
            }
        }

        public class Int32 : Base<int>
        {
            public Int32(Func<TItem, int?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, int? obj)
            {
                return hasher.Hash(seed, obj);
            }
        }

        public class UInt32 : Base<uint>
        {
            public UInt32(Func<TItem, uint?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, uint? obj)
            {
                return hasher.Hash(seed, obj);
            }
        }

        public class Int64 : Base<long>
        {
            public Int64(Func<TItem, long?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, long? obj)
            {
                return hasher.Hash(seed, obj);
            }
        }

        public class UInt64 : Base<ulong>
        {
            public UInt64(Func<TItem, ulong?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, ulong? obj)
            {
                return hasher.Hash(seed, obj);
            }
        }

        public class Int16 : Base<short>
        {
            public Int16(Func<TItem, short?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, short? obj)
            {
                return hasher.Hash(seed, obj);
            }
        }

        public class UInt16 : Base<ushort>
        {
            public UInt16(Func<TItem, ushort?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, ushort? obj)
            {
                return hasher.Hash(seed, obj);
            }
        }

        #endregion
    }
}
