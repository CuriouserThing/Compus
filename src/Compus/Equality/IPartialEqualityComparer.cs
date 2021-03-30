namespace Compus.Equality
{
    public interface IPartialEqualityComparer<in T>
    {
        int ContinueHashCode(IHasher hasher, int seed, T obj);

        bool PartEquals(T x, T y);
    }
}
