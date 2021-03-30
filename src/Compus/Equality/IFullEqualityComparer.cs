using System.Collections.Generic;

namespace Compus.Equality
{
    public interface IFullEqualityComparer<in T> : IEqualityComparer<T>,
                                                   IPartialEqualityComparer<T>
    {
        bool Equals(T x, object? y)
        {
            return y is T other && (this as IEqualityComparer<T>).Equals(x, other);
        }
    }
}
