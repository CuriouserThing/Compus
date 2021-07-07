using System;

namespace Compus.Caching
{
    internal interface IEvictionPolicy
    {
        bool CanEvictOldest(TimeSpan oldestLifespan, int cacheSize);
    }
}
