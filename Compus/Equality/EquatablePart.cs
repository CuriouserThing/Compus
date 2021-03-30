namespace Compus.Equality
{
    public abstract class EquatablePart<T> : IEquatablePart<T> where T : EquatablePart<T>
    {
        protected abstract IFullEqualityComparer<T> EqualityComparer { get; }

        public bool Equals(T? other)
        {
            return EqualityComparer.Equals((T)this, other);
        }

        public int ContinueHashCode(IHasher hasher, int seed)
        {
            return EqualityComparer.ContinueHashCode(hasher, seed, (T)this);
        }

        public sealed override bool Equals(object? obj)
        {
            return EqualityComparer.Equals((T)this, obj);
        }

        public sealed override int GetHashCode()
        {
            return EqualityComparer.GetHashCode((T)this);
        }
    }
}
