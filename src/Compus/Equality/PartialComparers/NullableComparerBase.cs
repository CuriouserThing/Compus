using System;

namespace Compus.Equality.PartialComparers
{
    internal abstract class NullableComparerBase<TItem, TPart> : IPartialEqualityComparer<TItem>
        where TItem : notnull
    {
        private readonly Func<TItem, TPart?> _selectPart;

        protected NullableComparerBase(Func<TItem, TPart?> selectPart)
        {
            _selectPart = selectPart;
        }

        public int ContinueHashCode(IHasher hasher, int seed, TItem obj)
        {
            TPart? part = _selectPart(obj);
            return ContinueHashCode(hasher, seed, part);
        }

        public bool PartEquals(TItem x, TItem y)
        {
            TPart? px = _selectPart(x);
            TPart? py = _selectPart(y);
            return PartEquals(px, py);
        }

        protected abstract int ContinueHashCode(IHasher hasher, int seed, TPart? obj);

        protected abstract bool PartEquals(TPart? x, TPart? y);
    }
}
