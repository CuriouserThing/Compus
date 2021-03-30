using System;
using System.Collections.Generic;
using Compus.Equality.PartialComparers;

namespace Compus.Equality
{
    public sealed class Identity<TItem> where TItem : notnull
    {
        private readonly List<IPartialEqualityComparer<TItem>> _comparers = new();

        public IFullEqualityComparer<TItem> ToComparer()
        {
            return ToComparer(new Fnv1aHasher());
        }

        public IFullEqualityComparer<TItem> ToComparer(IHasher hasher)
        {
            if (_comparers.Count == 1)
            {
                return new SinglePartIdentityComparer<TItem>(hasher, _comparers[0]);
            }
            else
            {
                return new IdentityComparer<TItem>(hasher, _comparers.ToArray());
            }
        }

        private Identity<TItem> FluentAdd(IPartialEqualityComparer<TItem> comparer)
        {
            _comparers.Add(comparer);
            return this;
        }

        public Identity<TItem> With(IPartialEqualityComparer<TItem?> comparer)
        {
            return FluentAdd(comparer);
        }

        public Identity<TItem> With(Func<TItem, string?> selectPart)
        {
            return FluentAdd(new StringComparer<TItem>(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<string?>?> selectPart)
        {
            return FluentAdd(new StringSequenceComparer<TItem>(selectPart));
        }

        #region Generics

        public Identity<TItem> With<TPart>(Func<TItem, TPart?> selectPart)
            where TPart : IEquatablePart<TPart>
        {
            return FluentAdd(new AutoComparer<TItem, TPart>(selectPart));
        }

        public Identity<TItem> WithSequence<TPart>(Func<TItem, IEnumerable<TPart?>?> selectPart)
            where TPart : IEquatablePart<TPart>
        {
            return FluentAdd(new AutoSequenceComparer<TItem, TPart>(selectPart));
        }

        public Identity<TItem> With<TPart>(Func<TItem, TPart?> selectPart, IPartialEqualityComparer<TPart> partComparer)
            where TPart : notnull
        {
            return FluentAdd(new ManualComparer<TItem, TPart>(selectPart, partComparer));
        }

        public Identity<TItem> WithSequence<TPart>(Func<TItem, IEnumerable<TPart?>?> selectPart, IPartialEqualityComparer<TPart> partComparer)
            where TPart : notnull
        {
            return FluentAdd(new ManualSequenceComparer<TItem, TPart>(selectPart, partComparer));
        }

        public Identity<TItem> WithReference<TPart>(Func<TItem, TPart?> selectPart)
            where TPart : class
        {
            return FluentAdd(new ReferenceComparer<TItem, TPart>(selectPart));
        }

        public Identity<TItem> WithReferenceSequence<TPart>(Func<TItem, IEnumerable<TPart?>?> selectPart)
            where TPart : class
        {
            return FluentAdd(new ReferenceSequenceComparer<TItem, TPart>(selectPart));
        }

        #endregion

        #region Primitives

        public Identity<TItem> With(Func<TItem, bool> selectPart)
        {
            return FluentAdd(new StructComparers<TItem>.Boolean(selectPart));
        }

        public Identity<TItem> With(Func<TItem, byte> selectPart)
        {
            return FluentAdd(new StructComparers<TItem>.Byte(selectPart));
        }

        public Identity<TItem> With(Func<TItem, sbyte> selectPart)
        {
            return FluentAdd(new StructComparers<TItem>.SByte(selectPart));
        }

        public Identity<TItem> With(Func<TItem, char> selectPart)
        {
            return FluentAdd(new StructComparers<TItem>.Char(selectPart));
        }

        public Identity<TItem> With(Func<TItem, double> selectPart)
        {
            return FluentAdd(new StructComparers<TItem>.Double(selectPart));
        }

        public Identity<TItem> With(Func<TItem, float> selectPart)
        {
            return FluentAdd(new StructComparers<TItem>.Single(selectPart));
        }

        public Identity<TItem> With(Func<TItem, int> selectPart)
        {
            return FluentAdd(new StructComparers<TItem>.Int32(selectPart));
        }

        public Identity<TItem> With(Func<TItem, uint> selectPart)
        {
            return FluentAdd(new StructComparers<TItem>.UInt32(selectPart));
        }

        public Identity<TItem> With(Func<TItem, long> selectPart)
        {
            return FluentAdd(new StructComparers<TItem>.Int64(selectPart));
        }

        public Identity<TItem> With(Func<TItem, ulong> selectPart)
        {
            return FluentAdd(new StructComparers<TItem>.UInt64(selectPart));
        }

        public Identity<TItem> With(Func<TItem, short> selectPart)
        {
            return FluentAdd(new StructComparers<TItem>.Int16(selectPart));
        }

        public Identity<TItem> With(Func<TItem, ushort> selectPart)
        {
            return FluentAdd(new StructComparers<TItem>.UInt16(selectPart));
        }

        #endregion

        #region Nullable Primitives

        public Identity<TItem> With(Func<TItem, bool?> selectPart)
        {
            return FluentAdd(new NullableStructComparers<TItem>.Boolean(selectPart));
        }

        public Identity<TItem> With(Func<TItem, byte?> selectPart)
        {
            return FluentAdd(new NullableStructComparers<TItem>.Byte(selectPart));
        }

        public Identity<TItem> With(Func<TItem, sbyte?> selectPart)
        {
            return FluentAdd(new NullableStructComparers<TItem>.SByte(selectPart));
        }

        public Identity<TItem> With(Func<TItem, char?> selectPart)
        {
            return FluentAdd(new NullableStructComparers<TItem>.Char(selectPart));
        }

        public Identity<TItem> With(Func<TItem, double?> selectPart)
        {
            return FluentAdd(new NullableStructComparers<TItem>.Double(selectPart));
        }

        public Identity<TItem> With(Func<TItem, float?> selectPart)
        {
            return FluentAdd(new NullableStructComparers<TItem>.Single(selectPart));
        }

        public Identity<TItem> With(Func<TItem, int?> selectPart)
        {
            return FluentAdd(new NullableStructComparers<TItem>.Int32(selectPart));
        }

        public Identity<TItem> With(Func<TItem, uint?> selectPart)
        {
            return FluentAdd(new NullableStructComparers<TItem>.UInt32(selectPart));
        }

        public Identity<TItem> With(Func<TItem, long?> selectPart)
        {
            return FluentAdd(new NullableStructComparers<TItem>.Int64(selectPart));
        }

        public Identity<TItem> With(Func<TItem, ulong?> selectPart)
        {
            return FluentAdd(new NullableStructComparers<TItem>.UInt64(selectPart));
        }

        public Identity<TItem> With(Func<TItem, short?> selectPart)
        {
            return FluentAdd(new NullableStructComparers<TItem>.Int16(selectPart));
        }

        public Identity<TItem> With(Func<TItem, ushort?> selectPart)
        {
            return FluentAdd(new NullableStructComparers<TItem>.UInt16(selectPart));
        }

        #endregion

        #region Primitive Sequences

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<bool>?> selectPart)
        {
            return FluentAdd(new StructSequenceComparers<TItem>.Boolean(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<byte>?> selectPart)
        {
            return FluentAdd(new StructSequenceComparers<TItem>.Byte(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<sbyte>?> selectPart)
        {
            return FluentAdd(new StructSequenceComparers<TItem>.SByte(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<char>?> selectPart)
        {
            return FluentAdd(new StructSequenceComparers<TItem>.Char(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<double>?> selectPart)
        {
            return FluentAdd(new StructSequenceComparers<TItem>.Double(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<float>?> selectPart)
        {
            return FluentAdd(new StructSequenceComparers<TItem>.Single(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<int>?> selectPart)
        {
            return FluentAdd(new StructSequenceComparers<TItem>.Int32(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<uint>?> selectPart)
        {
            return FluentAdd(new StructSequenceComparers<TItem>.UInt32(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<long>?> selectPart)
        {
            return FluentAdd(new StructSequenceComparers<TItem>.Int64(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<ulong>?> selectPart)
        {
            return FluentAdd(new StructSequenceComparers<TItem>.UInt64(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<short>?> selectPart)
        {
            return FluentAdd(new StructSequenceComparers<TItem>.Int16(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<ushort>?> selectPart)
        {
            return FluentAdd(new StructSequenceComparers<TItem>.UInt16(selectPart));
        }

        #endregion

        #region Nullable Primitive Sequences

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<bool?>?> selectPart)
        {
            return FluentAdd(new NullableStructSequenceComparers<TItem>.Boolean(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<byte?>?> selectPart)
        {
            return FluentAdd(new NullableStructSequenceComparers<TItem>.Byte(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<sbyte?>?> selectPart)
        {
            return FluentAdd(new NullableStructSequenceComparers<TItem>.SByte(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<char?>?> selectPart)
        {
            return FluentAdd(new NullableStructSequenceComparers<TItem>.Char(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<double?>?> selectPart)
        {
            return FluentAdd(new NullableStructSequenceComparers<TItem>.Double(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<float?>?> selectPart)
        {
            return FluentAdd(new NullableStructSequenceComparers<TItem>.Single(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<int?>?> selectPart)
        {
            return FluentAdd(new NullableStructSequenceComparers<TItem>.Int32(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<uint?>?> selectPart)
        {
            return FluentAdd(new NullableStructSequenceComparers<TItem>.UInt32(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<long?>?> selectPart)
        {
            return FluentAdd(new NullableStructSequenceComparers<TItem>.Int64(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<ulong?>?> selectPart)
        {
            return FluentAdd(new NullableStructSequenceComparers<TItem>.UInt64(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<short?>?> selectPart)
        {
            return FluentAdd(new NullableStructSequenceComparers<TItem>.Int16(selectPart));
        }

        public Identity<TItem> WithSequence(Func<TItem, IEnumerable<ushort?>?> selectPart)
        {
            return FluentAdd(new NullableStructSequenceComparers<TItem>.UInt16(selectPart));
        }

        #endregion
    }
}
