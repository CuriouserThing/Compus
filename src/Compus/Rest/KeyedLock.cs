using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Compus.Rest;

internal class KeyedLock<T> : IDisposable where T : notnull
{
    private readonly BlockingCollection<SemaphoreSlim> _semaphorePool;
    private readonly Dictionary<T, CountedSemaphore> _semaphores;

    public KeyedLock(int semaphorePoolSize)
    {
        _semaphorePool = new BlockingCollection<SemaphoreSlim>(new ConcurrentBag<SemaphoreSlim>(), semaphorePoolSize);
        _semaphores = new Dictionary<T, CountedSemaphore>();
    }

    public void Dispose()
    {
        _semaphorePool.Dispose();
    }

    public async Task WaitAsync(T key)
    {
        SemaphoreSlim semaphore;
        lock (_semaphores)
        {
            if (_semaphores.TryGetValue(key, out CountedSemaphore? cs))
            {
                semaphore = cs.Semaphore;
                cs.RefCount += 1;
            }
            else
            {
                semaphore = _semaphorePool.TryTake(out SemaphoreSlim? s) ? s : new SemaphoreSlim(1, 1);
                _semaphores.Add(key, new CountedSemaphore(semaphore));
            }
        }

        await semaphore.WaitAsync();
    }

    public void Release(T key)
    {
        SemaphoreSlim semaphore;
        bool dispose;
        lock (_semaphores)
        {
            CountedSemaphore cs = _semaphores[key];
            semaphore = cs.Semaphore;
            if (cs.RefCount < 2)
            {
                _semaphores.Remove(key);
                dispose = true;
            }
            else
            {
                cs.RefCount -= 1;
                dispose = false;
            }
        }

        semaphore.Release();
        if (dispose && !_semaphorePool.TryAdd(semaphore))
        {
            semaphore.Dispose();
        }
    }

    private class CountedSemaphore
    {
        public CountedSemaphore(SemaphoreSlim semaphore)
        {
            Semaphore = semaphore;
            RefCount = 1;
        }

        public SemaphoreSlim Semaphore { get; }
        public int RefCount { get; set; }
    }
}
