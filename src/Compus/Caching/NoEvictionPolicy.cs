using System;

namespace Compus.Caching
{
    internal class NoEvictionPolicy : IEvictionPolicy
    {
        public bool CanEvictOldest(TimeSpan oldestLifespan, int cacheSize)
        {
            return false;
        }
    }
}
