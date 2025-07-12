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

public interface IKeyedEvent<in TKey, TMessage> : IKeyedPublisher<TKey, TMessage>, IKeyedSubscriber<TKey, TMessage>, IKeyedEvent
    where TKey : notnull { }

public sealed class KeyedEvent<TKey, TMessage> : IKeyedEvent<TKey, TMessage> where TKey : notnull {
    readonly Dictionary<TKey, IEvent<TMessage>> _events = new();

    public IDisposable Subscribe(TKey key, IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) {
        if (_events.TryGetValue(key, out var @event)) 
            return @event.Subscribe(observer, filters);
        
        @event = new Event<TMessage>();
        _events[key] = @event;
        return @event.Subscribe(observer, filters);
    }

    public void Publish(TKey key, TMessage message) {
        if (_events.TryGetValue(key, out var @event)) {
            @event.Publish(message);
        }
    }

    ~KeyedEvent() {
        Dispose();
    }
    
    public void Dispose() {
        foreach (var @event in _events.Values) {
            @event.Dispose();
        }

        _events.Clear();
        GC.SuppressFinalize(this);
    }
}