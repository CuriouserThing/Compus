using System;

namespace Compus.Equality.PartialComparers
{
    internal sealed class ManualComparer<TItem, TPart> : NullableComparerBase<TItem, TPart?>
        where TItem : notnull
    {
        private readonly IPartialEqualityComparer<TPart> _partComparer;

        public ManualComparer(Func<TItem, TPart?> selectPart, IPartialEqualityComparer<TPart> partComparer) : base(selectPart)
        {
            _partComparer = partComparer;
        }

        protected override int ContinueHashCode(IHasher hasher, int seed, TPart? obj)
        {
            return hasher.Hash(seed, obj, _partComparer);
        }

        protected override bool PartEquals(TPart? x, TPart? y)
        {
            if (x is null)
            {
                return y is null;
            }
            else
            {
                return y is not null && _partComparer.PartEquals(x, y);
            }
        }
    }
}
