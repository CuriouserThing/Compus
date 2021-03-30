using System;

namespace Compus.Equality.PartialComparers
{
    internal class StringComparer<TItem> : NullableComparerBase<TItem, string?>
        where TItem : notnull
    {
        public StringComparer(Func<TItem, string?> selectPart) : base(selectPart)
        {
        }

        protected override int ContinueHashCode(IHasher hasher, int seed, string? obj)
        {
            return hasher.Hash(seed, obj);
        }

        protected override bool PartEquals(string? x, string? y)
        {
            if (x is null)
            {
                return y is null;
            }
            else
            {
                return y is not null && x.Equals(y);
            }
        }
    }
}
