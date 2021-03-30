using System.Collections.Generic;
using System.Linq;

namespace Compus.Equality
{
    internal sealed class IdentityComparer<T> : IFullEqualityComparer<T>
    {
        private readonly IPartialEqualityComparer<T>[] _comparers;
        private readonly IHasher _hasher;

        internal IdentityComparer(IHasher hasher, IPartialEqualityComparer<T>[] comparers)
        {
            _hasher    = hasher;
            _comparers = comparers;
        }

        bool IEqualityComparer<T>.Equals(T? x, T? y)
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

        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            return ContinueHashCode(_hasher, _hasher.Start(), obj);
        }

        public int ContinueHashCode(IHasher hasher, int seed, T obj)
        {
            return _comparers.Aggregate(seed, (current, comparer) => comparer.ContinueHashCode(hasher, current, obj));
        }

        public bool PartEquals(T x, T y)
        {
            return _comparers.All(comparer => comparer.PartEquals(x, y));
        }
    }
}
