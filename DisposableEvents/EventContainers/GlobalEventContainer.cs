namespace DisposableEvents.EventContainers;

public static class GlobalEventContainer {
    static Lazy<IEventContainer> s_lazyEventContainer = new Lazy<IEventContainer>(() => new EventContainer());
    static IEventContainer EventContainer => s_lazyEventContainer.Value;

    public static void Configure(IEventContainer eventContainer) {
        if (eventContainer == null) throw new ArgumentNullException(nameof(eventContainer));
        
        if (s_lazyEventContainer.IsValueCreated)
            throw new InvalidOperationException("Global event container is already configured.");
        
        s_lazyEventContainer = new Lazy<IEventContainer>(() => eventContainer);
    }

    // Unkeyed
    public static void RegisterEvent<TMessage>(IEvent<TMessage>? @event = null) {
        EventContainer.RegisterEvent<TMessage>(@event);
    }

    public static bool TryGetEvent<TMessage>(out IEvent<TMessage> @event) {
        return EventContainer.TryGetEvent<TMessage>(out @event);
    }
    
    public static void Publish<TMessage>(TMessage message) {
        EventContainer.Publish(message);
    }
    
    public static IDisposable Subscribe<TMessage>(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) {
        return EventContainer.Subscribe(observer, filters);
    }
    
    // Keyed
    public static void RegisterEvent<TKey, TMessage>(IKeyedEvent<TKey, TMessage>? @event = null) where TKey : notnull {
        EventContainer.RegisterEvent(@event);
    }
    
    public static bool TryGetEvent<TKey, TMessage>(out IKeyedEvent<TKey, TMessage> @event) where TKey : notnull {
        return EventContainer.TryGetEvent(out @event);
    }
    
    public static void Publish<TKey, TMessage>(TKey key, TMessage message) where TKey : notnull {
        EventContainer.Publish(key, message);
    }
    
    public static IDisposable Subscribe<TKey, TMessage>(TKey key, IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) where TKey : notnull {
        return EventContainer.Subscribe(key, observer, filters);
    }
}