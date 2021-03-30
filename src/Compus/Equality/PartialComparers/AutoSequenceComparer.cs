using System;
using System.Collections.Generic;

namespace Compus.Equality.PartialComparers
{
    internal sealed class AutoSequenceComparer<TItem, TPart> : SequenceComparerBase<TItem, TPart>
        where TItem : notnull
        where TPart : IEquatablePart<TPart>
    {
        public AutoSequenceComparer(Func<TItem, IEnumerable<TPart?>?> selectPart) : base(selectPart)
        {
        }

        protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<TPart?>? obj)
        {
            return hasher.HashSequence(seed, obj);
        }

        protected override bool PartEquals(TPart x, TPart y)
        {
            return x.Equals(y);
        }
    }
}
