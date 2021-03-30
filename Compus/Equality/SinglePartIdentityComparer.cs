using System.Collections.Generic;

namespace Compus.Equality
{
    internal sealed class SinglePartIdentityComparer<T> : IFullEqualityComparer<T>
    {
        private readonly IPartialEqualityComparer<T> _comparer;
        private readonly IHasher _hasher;

        internal SinglePartIdentityComparer(IHasher hasher, IPartialEqualityComparer<T> comparer)
        {
            _hasher   = hasher;
            _comparer = comparer;
        }

        bool IEqualityComparer<T>.Equals(T? x, T? y)
        {
            if (x is null)
            {
                return y is null;
            }
            else
            {
                return y is not null && _comparer.PartEquals(x, y);
            }
        }

        int IEqualityComparer<T>.GetHashCode(T obj)
        {
            return _comparer.ContinueHashCode(_hasher, _hasher.Start(), obj);
        }

        public int ContinueHashCode(IHasher hasher, int seed, T obj)
        {
            return _comparer.ContinueHashCode(hasher, seed, obj);
        }

        public bool PartEquals(T x, T y)
        {
            return _comparer.PartEquals(x, y);
        }
    }
}
