using System;

namespace Compus.Equality
{
    public interface IEquatablePart<T> : IEquatable<T>
    {
        int ContinueHashCode(IHasher hasher, int seed);
    }
}
