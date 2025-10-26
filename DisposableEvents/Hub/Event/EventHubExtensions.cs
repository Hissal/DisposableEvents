namespace DisposableEvents;

public static class EventHubExtensions {
    public static void Publish<TMessage>(this IEventHub hub, TMessage message) {
        if (hub.TryGetEvent<TMessage>(out var eventInstance)) {
            eventInstance.Publish(message);
        }
    }
    
    // ----- Subscribe Overloads ----- //
    public static IDisposable Subscribe<TMessage>(this IEventHub hub, IEventHandler<TMessage> handler) {
        var eventInstance = hub.GetEvent<TMessage>() 
                            ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return eventInstance.Subscribe(handler);
    }
    
    public static IDisposable Subscribe<TMessage>(this IEventHub hub, Action<TMessage> handler) {
        var ev = hub.GetEvent<TMessage>() 
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler);
    }
    
    public static IDisposable Subscribe<TMessage>(
        this IEventHub hub,
        IEventHandler<TMessage> handler,
        IEventFilter<TMessage> filter)
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler, filter);
    }
    
    public static IDisposable Subscribe<TMessage>(
        this IEventHub hub,
        IEventHandler<TMessage> handler,
        IEventFilter<TMessage>[] filters,
        FilterOrdering ordering)
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler, filters, ordering);
    }
    
    public static IDisposable Subscribe<TMessage>(
        this IEventHub hub,
        IEventHandler<TMessage> handler,
        params IEventFilter<TMessage>[] filters)
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler, filters);
    }
    
    public static IDisposable Subscribe<TMessage>(
        this IEventHub hub,
        Action<TMessage> handler,
        params IEventFilter<TMessage>[] filters)
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler, filters);
    }
    
    public static IDisposable Subscribe<TMessage>(
        this IEventHub hub,
        Action<TMessage> handler,
        Func<TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters)
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler, predicateFilter, additionalFilters);
    }

    // ----- Stateful Subscribe Overloads -----
    public static IDisposable Subscribe<TState, TMessage>(
        this IEventHub hub,
        TState state,
        Action<TState, TMessage> handler,
        params IEventFilter<TMessage>[] filters)
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(state, handler, filters);
    }

    public static IDisposable Subscribe<TState, TMessage>(
        this IEventHub hub,
        TState state,
        Action<TState, TMessage> handler,
        Func<TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters)
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(state, handler, predicateFilter, additionalFilters);
    }

    public static IDisposable Subscribe<TState, TMessage>(
        this IEventHub hub,
        TState state,
        Action<TState, TMessage> handler,
        Func<TState, TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters)
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(state, handler, predicateFilter, additionalFilters);
    }
}