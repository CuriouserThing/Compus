using System;

namespace Compus.Equality.PartialComparers
{
    internal sealed class AutoComparer<TItem, TPart> : NullableComparerBase<TItem, TPart?>
        where TItem : notnull
        where TPart : IEquatablePart<TPart>
    {
        public AutoComparer(Func<TItem, TPart?> selectPart) : base(selectPart)
        {
        }

        protected override int ContinueHashCode(IHasher hasher, int seed, TPart? obj)
        {
            return hasher.Hash(seed, obj);
        }

        protected override bool PartEquals(TPart? x, TPart? y)
        {
            if (x is null)
            {
                return y is null;
            }
            else
            {
                return x.Equals(y);
            }
        }
    }
}
