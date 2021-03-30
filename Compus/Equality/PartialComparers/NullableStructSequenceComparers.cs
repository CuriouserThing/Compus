using System;
using System.Collections.Generic;
using System.Linq;

namespace Compus.Equality.PartialComparers
{
    internal static class NullableStructSequenceComparers<TItem> where TItem : notnull
    {
        public abstract class Base<TPart> : NullableComparerBase<TItem, IEnumerable<TPart?>?>
            where TPart : struct, IEquatable<TPart>
        {
            protected Base(Func<TItem, IEnumerable<TPart?>?> selectPart) : base(selectPart)
            {
            }

            protected sealed override bool PartEquals(IEnumerable<TPart?>? x, IEnumerable<TPart?>? y)
            {
                if (x is null)
                {
                    return y is null;
                }
                else
                {
                    return y is not null && x.SequenceEqual(y);
                }
            }
        }

        #region Implementations

        public class Boolean : Base<bool>
        {
            public Boolean(Func<TItem, IEnumerable<bool?>?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<bool?>? obj)
            {
                return hasher.HashSequence(seed, obj);
            }
        }

        public class Byte : Base<byte>
        {
            public Byte(Func<TItem, IEnumerable<byte?>?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<byte?>? obj)
            {
                return hasher.HashSequence(seed, obj);
            }
        }

        public class SByte : Base<sbyte>
        {
            public SByte(Func<TItem, IEnumerable<sbyte?>?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<sbyte?>? obj)
            {
                return hasher.HashSequence(seed, obj);
            }
        }

        public class Char : Base<char>
        {
            public Char(Func<TItem, IEnumerable<char?>?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<char?>? obj)
            {
                return hasher.HashSequence(seed, obj);
            }
        }

        public class Double : Base<double>
        {
            public Double(Func<TItem, IEnumerable<double?>?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<double?>? obj)
            {
                return hasher.HashSequence(seed, obj);
            }
        }

        public class Single : Base<float>
        {
            public Single(Func<TItem, IEnumerable<float?>?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<float?>? obj)
            {
                return hasher.HashSequence(seed, obj);
            }
        }

        public class Int32 : Base<int>
        {
            public Int32(Func<TItem, IEnumerable<int?>?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<int?>? obj)
            {
                return hasher.HashSequence(seed, obj);
            }
        }

        public class UInt32 : Base<uint>
        {
            public UInt32(Func<TItem, IEnumerable<uint?>?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<uint?>? obj)
            {
                return hasher.HashSequence(seed, obj);
            }
        }

        public class Int64 : Base<long>
        {
            public Int64(Func<TItem, IEnumerable<long?>?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<long?>? obj)
            {
                return hasher.HashSequence(seed, obj);
            }
        }

        public class UInt64 : Base<ulong>
        {
            public UInt64(Func<TItem, IEnumerable<ulong?>?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<ulong?>? obj)
            {
                return hasher.HashSequence(seed, obj);
            }
        }

        public class Int16 : Base<short>
        {
            public Int16(Func<TItem, IEnumerable<short?>?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<short?>? obj)
            {
                return hasher.HashSequence(seed, obj);
            }
        }

        public class UInt16 : Base<ushort>
        {
            public UInt16(Func<TItem, IEnumerable<ushort?>?> selectPart) : base(selectPart)
            {
            }

            protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<ushort?>? obj)
            {
                return hasher.HashSequence(seed, obj);
            }
        }

        #endregion
    }
}
