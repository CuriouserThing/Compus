using System;

namespace Compus.Caching
{
    internal class NoGrowthPolicy : IEvictionPolicy
    {
        public bool CanEvictOldest(TimeSpan oldestLifespan, int cacheSize)
        {
            return true;
        }
    }
}
