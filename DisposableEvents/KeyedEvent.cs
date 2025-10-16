using DisposableEvents.Disposables;
using DisposableEvents.Factories;

namespace DisposableEvents;

public interface IKeyedEventPublisher<in TKey, in TMessage> where TKey : notnull {
    /// <summary>
    /// Publishes a message to the event with the specified key.
    /// </summary>
    /// <param name="key">The key of the event to publish to.</param>
    /// <param name="message">The message to publish.</param>
    void Publish(TKey key, TMessage message);
}

public interface IKeyedEventSubscriber<in TKey, TMessage> where TKey : notnull {
    /// <summary>
    /// Subscribes to the event with the specified key.
    /// </summary>
    /// <param name="key">The key to subscribe to.</param>
    /// <param name="handler">The observer to notify when the event is published.</param>
    /// <param name="filters"></param>
    /// <returns>A disposable that can be used to unsubscribe from the event.</returns>
    IDisposable Subscribe(TKey key, IEventHandler<TMessage> handler, params IEventFilter<TMessage>[] filters);
}

public interface IKeyedEvent : IDisposable { }

public interface IKeyedEvent<in TKey, TMessage> : IKeyedEventPublisher<TKey, TMessage>, IKeyedEventSubscriber<TKey, TMessage>,
    IKeyedEvent
    where TKey : notnull {
    void Dispose(TKey key);
}

public sealed class KeyedEvent<TKey, TMessage> : IKeyedEvent<TKey, TMessage> where TKey : notnull {
    readonly Dictionary<TKey, IDisposableEvent<TMessage>> events = new();
    readonly IFilteredEventHandlerFactory handlerFactory;
    readonly int expectedSubscriberCount;
    
    bool isDisposed;

    public KeyedEvent(int expectedSubscriberCountPerKey = 2) 
        : this(expectedSubscriberCountPerKey, FilteredEventHandlerFactory.Default) { }
    public KeyedEvent(int expectedSubscriberCountPerKey, IFilteredEventHandlerFactory handlerFactory) {
        expectedSubscriberCount = expectedSubscriberCountPerKey;
        this.handlerFactory = handlerFactory;
    }
    
    public IDisposable Subscribe(TKey key, IEventHandler<TMessage> handler, params IEventFilter<TMessage>[] filters) {
        if (isDisposed) {
            handler?.OnUnsubscribe();
            return Disposable.Empty;
        }
        
        if (events.TryGetValue(key, out var @event)) 
            return @event.Subscribe(handler, filters);
        
        @event = CreateEvent(key);
        return @event.Subscribe(handler, filters);
    }

    public void Publish(TKey key, TMessage message) {
        if (events.TryGetValue(key, out var @event)) {
            @event.Publish(message);
        }
    }

    DisposableEvent<TMessage> CreateEvent(TKey key) {
        var @event = new DisposableEvent<TMessage>(expectedSubscriberCount);
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