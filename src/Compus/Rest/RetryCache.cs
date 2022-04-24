using System;
using System.Collections.Generic;
using Compus.Caching;

namespace Compus.Rest;

internal class RetryCache<TResource>
{
    private readonly Cache<TResource, long> _sharedResourceTimes;
    private readonly Cache<ResourceBucket, long> _userResourceTimes;

    public RetryCache(IEvictionPolicy evictionPolicy)
    {
        _sharedResourceTimes = new Cache<TResource, long>(ResourceComparer, evictionPolicy);
        _userResourceTimes = new Cache<ResourceBucket, long>(BucketComparer, evictionPolicy);
    }

    private static IEqualityComparer<TResource> ResourceComparer { get; } = EqualityComparer<TResource>.Default;
    private static IEqualityComparer<ResourceBucket> BucketComparer { get; } = EqualityComparer<ResourceBucket>.Default;

    public long GetRetry(TResource resource, string? bucket)
    {
        long retry = 0;
        lock (_sharedResourceTimes)
        {
            if (_sharedResourceTimes.TryGetValue(resource, out Cached<long> sharedRetry))
            {
                retry = sharedRetry;
            }
        }

        if (bucket is null) { return retry; }

        var resourceBucket = new ResourceBucket(resource, bucket);
        lock (_userResourceTimes)
        {
            if (_userResourceTimes.TryGetValue(resourceBucket, out Cached<long> userRetry) &&
                retry < userRetry)
            {
                retry = userRetry;
            }
        }

        return retry;
    }

    public void SetRetry(TResource resource, long retry)
    {
        lock (_sharedResourceTimes)
        {
            if (!_sharedResourceTimes.TryGetValue(resource, out Cached<long> sharedRetry))
            {
                _sharedResourceTimes.Add(resource, new Cached<long>(retry, DateTimeOffset.UtcNow));
            }
            else if (sharedRetry <= retry)
            {
                _sharedResourceTimes[resource] = new Cached<long>(retry, DateTimeOffset.UtcNow);
            }
        }
    }

    public void SetRetry(TResource resource, string bucket, long retry)
    {
        lock (_userResourceTimes)
        {
            var resourceBucket = new ResourceBucket(resource, bucket);
            if (!_userResourceTimes.TryGetValue(resourceBucket, out Cached<long> sharedRetryAfter))
            {
                _userResourceTimes.Add(resourceBucket, new Cached<long>(retry, DateTimeOffset.UtcNow));
            }
            else if (sharedRetryAfter <= retry)
            {
                _userResourceTimes[resourceBucket] = new Cached<long>(retry, DateTimeOffset.UtcNow);
            }
        }
    }

    private record struct ResourceBucket(TResource Resource, string Bucket);
}
