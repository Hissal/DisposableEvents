using DisposableEvents;
using DisposableEvents.EventContainers;
using DisposableEvents.Factories;

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
    
    public object? LastPublishedValueForKey(TKey key) {
        return PublishedDict.GetValueOrDefault(key);
    }
    
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

internal class TestUnkeyedEventContainer : IUnkeyedEventContainer {
    public readonly Dictionary<Type, IEvent> RegisteredEventsDict = new();
    public readonly List<IEvent> RegisteredEvents = new();
    public readonly List<object> PublishedMessages = new();
    public readonly List<object> SubscribedObservers = new();
    
    public object? LastPublishedValue => PublishedMessages.Count > 0 ? PublishedMessages[^1] : null;
    public bool IsDisposed;


    public IEvent<TMessage> RegisterEvent<TMessage>(IEvent<TMessage>? @event = null) {
        RegisteredEventsDict[typeof(TMessage)] = @event!;
        RegisteredEvents.Add(@event!);
        return @event!;
    }
    
    public bool TryGetEvent<TMessage>(out IEvent<TMessage> @event) {
        if (RegisteredEventsDict.TryGetValue(typeof(TMessage), out var e)) {
            @event = (IEvent<TMessage>)e;
            return true;
        }
        
        @event = null!;
        return false;
    }
    
    public void Publish<TMessage>(TMessage message) {
        PublishedMessages.Add(message!);
    }
    
    public IDisposable Subscribe<TMessage>(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) {
        SubscribedObservers.Add(observer);
        return new TestSubscription();
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

internal class TestKeyedEventContainer : IKeyedEventContainer {
    public readonly Dictionary<Type, IKeyedEvent> RegisteredEventsDict = new();
    public readonly List<IKeyedEvent> RegisteredEvents = new();
    public readonly Dictionary<object, object> PublishedMessagesDict = new();
    public readonly List<object> PublishedMessages = new();
    public readonly List<object> PublishedKeys = new();
    public readonly List<object> SubscribedObservers = new();
    public bool IsDisposed;
    
    public object? LastPublishedValue => PublishedMessages.Count > 0 ? PublishedMessages[^1] : null;
    public object? LastPublishedKey => PublishedKeys.Count > 0 ? PublishedKeys[^1] : null;
    public object? LastPublishedValueForKey(object key) {
        return PublishedMessagesDict.GetValueOrDefault(key);
    }

    public IKeyedEvent<TKey, TMessage> RegisterEvent<TKey, TMessage>(IKeyedEvent<TKey, TMessage>? @event = null) where TKey : notnull {
        RegisteredEventsDict[typeof(TMessage)] = @event!;
        RegisteredEvents.Add(@event!);
        return @event!;
    }
    
    public bool TryGetEvent<TKey, TMessage>(out IKeyedEvent<TKey, TMessage> @event) where TKey : notnull  {
        if (RegisteredEventsDict.TryGetValue(typeof(TMessage), out var e)) {
            @event = (IKeyedEvent<TKey, TMessage>)e;
            return true;
        }
        
        @event = null!;
        return false;
    }
    
    public void Publish<TKey, TMessage>(TKey key, TMessage message) where TKey : notnull  {
        PublishedMessagesDict[key] = message!;
        PublishedKeys.Add(key);
        PublishedMessages.Add(message!);
    }
    
    public IDisposable Subscribe<TKey, TMessage>(TKey key, IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) where TKey : notnull  {
        SubscribedObservers.Add(observer);
        return new TestSubscription();
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