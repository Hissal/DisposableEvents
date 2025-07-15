using DisposableEvents.Disposables;
using DisposableEvents.Factories;

namespace DisposableEvents;

public interface IKeyedPublisher<in TKey, in TMessage> where TKey : notnull {
    /// <summary>
    /// Publishes a message to the event with the specified key.
    /// </summary>
    /// <param name="key">The key of the event to publish to.</param>
    /// <param name="message">The message to publish.</param>
    void Publish(TKey key, TMessage message);
}

public interface IKeyedSubscriber<in TKey, TMessage> where TKey : notnull {
    /// <summary>
    /// Subscribes to the event with the specified key.
    /// </summary>
    /// <param name="key">The key to subscribe to.</param>
    /// <param name="observer">The observer to notify when the event is published.</param>
    /// <param name="filters"></param>
    /// <returns>A disposable that can be used to unsubscribe from the event.</returns>
    IDisposable Subscribe(TKey key, IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters);
}

public interface IKeyedEvent : IDisposable { }

public interface IKeyedEvent<in TKey, TMessage> : IKeyedPublisher<TKey, TMessage>, IKeyedSubscriber<TKey, TMessage>,
    IKeyedEvent
    where TKey : notnull {
    void Dispose(TKey key);
}

public sealed class KeyedEvent<TKey, TMessage> : IKeyedEvent<TKey, TMessage> where TKey : notnull {
    readonly Dictionary<TKey, IEvent<TMessage>> events = new();
    readonly IEventObserverFactory observerFactory;
    readonly int expectedSubscriberCount;
    
    bool isDisposed;

    public KeyedEvent(int expectedSubscriberCountPerKey = 2) 
        : this(expectedSubscriberCountPerKey, EventObserverFactory.Default) { }
    public KeyedEvent(int expectedSubscriberCountPerKey, IEventObserverFactory observerFactory) {
        expectedSubscriberCount = expectedSubscriberCountPerKey;
        this.observerFactory = observerFactory;
    }
    
    public IDisposable Subscribe(TKey key, IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) {
        if (isDisposed) {
            observer?.OnCompleted();
            return Disposable.Empty;
        }
        
        if (events.TryGetValue(key, out var @event)) 
            return @event.Subscribe(observer, filters);
        
        @event = CreateEvent(key);
        return @event.Subscribe(observer, filters);
    }

    public void Publish(TKey key, TMessage message) {
        if (events.TryGetValue(key, out var @event)) {
            @event.Publish(message);
        }
    }

    Event<TMessage> CreateEvent(TKey key) {
        var @event = new Event<TMessage>(expectedSubscriberCount, observerFactory);
        events[key] = @event;
        return @event;
    }

    ~KeyedEvent() {
        Dispose();
    }
    
    public void Dispose() {
        if (isDisposed)
            return;
        
        isDisposed = true;
        
        foreach (var @event in events.Values) {
            @event.Dispose();
        }

        events.Clear();
        GC.SuppressFinalize(this);
    }
    
    public void Dispose(TKey key) {
        if (!events.TryGetValue(key, out var @event))
            return;
        
        @event.Dispose();
        events.Remove(key);
    }
}