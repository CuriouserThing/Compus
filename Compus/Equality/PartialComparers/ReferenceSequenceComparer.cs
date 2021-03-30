using System;
using System.Collections.Generic;
using System.Linq;

namespace Compus.Equality.PartialComparers
{
    internal sealed class ReferenceSequenceComparer<TItem, TPart> : NullableComparerBase<TItem, IEnumerable<TPart?>?>
        where TItem : notnull
        where TPart : class
    {
        public ReferenceSequenceComparer(Func<TItem, IEnumerable<TPart?>?> selectPart) : base(selectPart)
        {
        }

        protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<TPart?>? obj)
        {
            return hasher.HashReferenceSequence(seed, obj);
        }

        protected override bool PartEquals(IEnumerable<TPart?>? x, IEnumerable<TPart?>? y)
        {
            if (x is null)
            {
                return y is null;
            }
            else
            {
                return y is not null && x.SequenceEqual(y, ReferenceEqualityComparer.Instance);
            }
        }
    }
}
