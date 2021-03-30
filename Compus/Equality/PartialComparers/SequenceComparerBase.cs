using System;
using System.Collections.Generic;

namespace Compus.Equality.PartialComparers
{
    internal abstract class SequenceComparerBase<TItem, TPart> : NullableComparerBase<TItem, IEnumerable<TPart?>>
        where TItem : notnull
    {
        public SequenceComparerBase(Func<TItem, IEnumerable<TPart?>?> selectPart) : base(selectPart)
        {
        }

        protected sealed override bool PartEquals(IEnumerable<TPart?>? x, IEnumerable<TPart?>? y)
        {
            if (x is null)
            {
                return y is null;
            }
            else
            {
                return y is not null && SequenceEquals(x, y);
            }
        }

        private bool SequenceEquals(IEnumerable<TPart?> xEnumerable, IEnumerable<TPart?> yEnumerable)
        {
            if (xEnumerable is ICollection<TPart?> xCollection && yEnumerable is ICollection<TPart?> yCollection)
            {
                int count = xCollection.Count;
                if (count != yCollection.Count)
                {
                    return false;
                }

                if (xCollection is IList<TPart?> xList && yCollection is IList<TPart?> yList)
                {
                    for (var i = 0; i < count; i++)
                    {
                        if (!NullablePartEquals(xList[i], yList[i]))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            using IEnumerator<TPart?> xEnumerator = xEnumerable.GetEnumerator();
            using IEnumerator<TPart?> yEnumerator = yEnumerable.GetEnumerator();
            while (xEnumerator.MoveNext())
            {
                if (!(yEnumerator.MoveNext() && NullablePartEquals(xEnumerator.Current, yEnumerator.Current)))
                {
                    return false;
                }
            }

            return !yEnumerator.MoveNext();
        }

        private bool NullablePartEquals(TPart? x, TPart? y)
        {
            if (x is null)
            {
                return y is null;
            }
            else
            {
                return y is not null && PartEquals(x, y);
            }
        }

        protected abstract bool PartEquals(TPart x, TPart y);
    }
}
