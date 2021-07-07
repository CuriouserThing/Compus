using System;

namespace Compus.Caching
{
    internal class LifespanEvictionPolicy : IEvictionPolicy
    {
        public LifespanEvictionPolicy(TimeSpan staleLifespan)
        {
            StaleLifespan = staleLifespan;
        }

        public TimeSpan StaleLifespan { get; }

        public bool CanEvictOldest(TimeSpan oldestLifespan, int cacheSize)
        {
            return oldestLifespan > StaleLifespan;
        }
    }
}
