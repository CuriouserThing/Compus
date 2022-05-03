using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Compus.Caching;

namespace Compus.Rest;

internal class ResourceLimiter<TScope>
{
    private readonly KeyedLock<ScopedBucket<TScope>> _resourceLock;
    private readonly Cache<ScopedBucket<TScope>, long> _sharedTimes;
    private readonly Cache<ScopedBucket<TScope>, long> _userTimes;
    private readonly object _cacheLock = new();

    public ResourceLimiter(int semaphorePoolSize, IEvictionPolicy evictionPolicy)
    {
        _resourceLock = new KeyedLock<ScopedBucket<TScope>>(semaphorePoolSize);
        _sharedTimes = new Cache<ScopedBucket<TScope>, long>(BucketComparer, evictionPolicy);
        _userTimes = new Cache<ScopedBucket<TScope>, long>(BucketComparer,   evictionPolicy);
    }

    private static IEqualityComparer<ScopedBucket<TScope>> BucketComparer { get; } = EqualityComparer<ScopedBucket<TScope>>.Default;

    public async Task<IDisposable> Lock(TScope scope, string bucket)
    {
        var key = new ScopedBucket<TScope>(scope, bucket);
        await _resourceLock.WaitAsync(key);
        return Disposable.Create(() => _resourceLock.Release(key));
    }

    public long GetRetry(TScope scope, string bucket)
    {
        lock (_cacheLock)
        {
            long retry = 0;
            var scopedBucket = new ScopedBucket<TScope>(scope, bucket);
            if (_sharedTimes.TryGetValue(scopedBucket, out Cached<long> sharedRetry))
            {
                retry = sharedRetry;
            }

            if (_userTimes.TryGetValue(scopedBucket, out Cached<long> userRetry) &&
                retry < userRetry)
            {
                return userRetry;
            }
            else
            {
                return retry;
            }
        }
    }

    public void SetRetry(TScope scope, string bucket, bool shared, long retry)
    {
        lock (_cacheLock)
        {
            Cache<ScopedBucket<TScope>, long> cache = shared ? _sharedTimes : _userTimes;
            var scopedBucket = new ScopedBucket<TScope>(scope, bucket);
            if (!cache.TryGetValue(scopedBucket, out Cached<long> oldRetry))
            {
                cache.Add(scopedBucket, new Cached<long>(retry, DateTimeOffset.UtcNow));
            }
            else if (oldRetry <= retry)
            {
                cache[scopedBucket] = new Cached<long>(retry, DateTimeOffset.UtcNow);
            }
        }
    }

    private record struct ScopedBucket<T>(T Scope, string Bucket);
}
