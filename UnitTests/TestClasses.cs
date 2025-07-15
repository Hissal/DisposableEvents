using DisposableEvents;
using DisposableEvents.Disposables;

namespace UnitTests;

internal class TestEvent<T> : IEvent<T> {
    public readonly List<T> Published = new();
    public T LastPublishedValue => Published.Count > 0 ? Published[^1] : default!;
    public bool IsDisposed;

    /// <summary>
    /// Return a no-op subscription that does nothing.
    /// </summary>
    public IDisposable Subscribe(IObserver<T> observer,  params IEventFilter<T>[] filters) {
        return new TestSubscription();
    }

    public void Publish(T value) {
        Published.Add(value);
    }
    
    public void Dispose() {
        IsDisposed = true;
    }

    class TestSubscription : IDisposable {
        public void Dispose() {
            // No-op for this test event
        }
    }
}

internal class TestKeyedEvent<TKey, T> : IKeyedEvent<TKey, T> where TKey : notnull {
    public readonly Dictionary<TKey, T> PublishedDict = new();
    public readonly List<T> Published = new();
    public readonly List<TKey> PublishedKeys = new();
    public readonly List<TKey> DisposedKeys = new();
    
    public T LastPublishedValue => Published.Count > 0 ? Published[^1] : default!;
    public TKey LastPublishedKey => PublishedKeys.Count > 0 ? PublishedKeys[^1] : default!;
    
    public bool IsDisposed;
    
    public void Publish(TKey key, T message) {
        PublishedDict[key] = message;
        Published.Add(message);
        PublishedKeys.Add(key);
    }
    
    /// <summary>
    /// Return a no-op subscription that does nothing.
    /// </summary>
    public IDisposable Subscribe(TKey key, IObserver<T> observer, params IEventFilter<T>[] filters) {
        return new TestSubscription();
    }
    public void Dispose(TKey key) {
        DisposedKeys.Add(key);
    }

    public void Dispose() {
        IsDisposed = true;
    }
    
    class TestSubscription : IDisposable {
        public void Dispose() {
            // No-op for this test event
        }
    }
}

internal class TestObserver<T> : IObserver<T> {
    public readonly List<T> Received = new();
    public T LastValue => Received.Count > 0 ? Received[^1] : default!;
    public Exception? Error;
    public bool Completed;

    public void OnNext(T value) => Received.Add(value);
    public void OnError(Exception error) => Error = error;
    public void OnCompleted() => Completed = true;
}

internal class ThrowingObserver<T> : IObserver<T> {
    public Exception? Error;
    readonly Exception toThrow;

    public ThrowingObserver(Exception ex) {
        toThrow = ex;
    }

    public void OnNext(T value) => throw toThrow;
    public void OnError(Exception error) => Error = error;
    public void OnCompleted() { }
}

internal class TestFilter<T> : IEventFilter<T> {
    public readonly List<T> Filtered = new();
    public T LastValue => Filtered.Count > 0 ? Filtered[^1] : default!;
    public Exception? Error;
    public bool Completed;

    public int FilterOrder { get; set; } = 0;

    public bool FilterEvent(ref T value) {
        Filtered.Add(value);
        return true;
    }

    public bool FilterOnError(Exception ex) {
        Error = ex;
        return true;
    }

    public bool FilterOnCompleted() {
        Completed = true;
        return true;
    }
}

internal class TestException : Exception {
    public TestException(string message) : base(message) { }
}