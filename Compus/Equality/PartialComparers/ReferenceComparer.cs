using System;

namespace Compus.Equality.PartialComparers
{
    internal sealed class ReferenceComparer<TItem, TPart> : NullableComparerBase<TItem, TPart?>
        where TItem : notnull
        where TPart : class
    {
        public ReferenceComparer(Func<TItem, TPart?> selectPart) : base(selectPart)
        {
        }

        protected override int ContinueHashCode(IHasher hasher, int seed, TPart? obj)
        {
            return hasher.HashReference(seed, obj);
        }

        protected override bool PartEquals(TPart? x, TPart? y)
        {
            return ReferenceEquals(x, y);
        }
    }
}
