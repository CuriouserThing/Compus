using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Microsoft.Extensions.Logging;

namespace Compus.Gateway
{
    internal class GatewayObservable<T> : IObservable<T>
    {
        private readonly object _lock = new();
        private readonly List<IObserver<T>> _observers = new();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            lock (_lock)
            {
                _observers.Add(observer);
            }

            return Disposable.Create(observer, Unsubscribe);
        }

        private void Unsubscribe(IObserver<T> observer)
        {
            lock (_lock)
            {
                bool removed = _observers.Remove(observer);
                if (removed)
                {
                    return;
                }

                if (_observers.Contains(observer))
                {
                    throw new Exception("Observer could not unsubscribe from gateway observable.");
                }
                else
                {
                    throw new ObjectDisposedException(null, "Observer already unsubscribed from gateway observable.");
                }
            }
        }

        public void Relay(T data, ILogger logger)
        {
            lock (_lock)
            {
                foreach (var observer in _observers)
                {
                    observer.OnNext(data);
                }
            }
        }
    }
}
