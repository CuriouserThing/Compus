using System;
using System.Collections.Generic;

namespace Compus.Equality.PartialComparers
{
    internal sealed class StringSequenceComparer<TItem> : SequenceComparerBase<TItem, string>
        where TItem : notnull
    {
        public StringSequenceComparer(Func<TItem, IEnumerable<string?>?> selectPart) : base(selectPart)
        {
        }

        protected override int ContinueHashCode(IHasher hasher, int seed, IEnumerable<string?>? obj)
        {
            return hasher.HashSequence(seed, obj);
        }

        protected override bool PartEquals(string x, string y)
        {
            return x.Equals(y);
        }
    }
}
