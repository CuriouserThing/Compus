using System;
using System.Collections.Generic;

namespace Compus.Equality.PartialComparers
{
    internal sealed class ManualSequenceComparer<TItem, TPart> : SequenceComparerBase<TItem, TPart>
        where TItem : notnull
    {
        private readonly IPartialEqualityComparer<TPart> _partComparer;

        public ManualSequenceComparer(Func<TItem, IEnumerable<TPart?>?> selectPart, IPartialEqualityComparer<TPart> partComparer) : base(selectPart)
        {
            _partComparer = partComparer;
        }

        protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<TPart?>? obj)
        {
            return hasher.HashSequence(seed, obj, _partComparer);
        }

        protected override bool PartEquals(TPart x, TPart y)
        {
            return _partComparer.PartEquals(x, y);
        }
    }
}
