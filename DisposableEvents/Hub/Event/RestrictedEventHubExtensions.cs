namespace DisposableEvents;

public static class RestrictedEventHubExtensions {
    public static void Publish<TMessageRestriction, TMessage>(this IEventHub<TMessageRestriction> hub, TMessage message)
        where TMessage : TMessageRestriction
    {
        if (hub.TryGetEvent<TMessage>(out var eventInstance)) {
            eventInstance.Publish(message);
        }
    }
    
    // ----- Subscribe Overloads ----- //
    public static IDisposable Subscribe<TMessageRestriction, TMessage>(
        this IEventHub<TMessageRestriction> hub,
        IEventHandler<TMessage> handler)
        where TMessage : TMessageRestriction
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler);
    }

    public static IDisposable Subscribe<TMessageRestriction, TMessage>(
        this IEventHub<TMessageRestriction> hub,
        Action<TMessage> handler)
        where TMessage : TMessageRestriction
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler);
    }

    public static IDisposable Subscribe<TMessageRestriction, TMessage>(
        this IEventHub<TMessageRestriction> hub,
        IEventHandler<TMessage> handler,
        IEventFilter<TMessage> filter)
        where TMessage : TMessageRestriction
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler, filter);
    }

    public static IDisposable Subscribe<TMessageRestriction, TMessage>(
        this IEventHub<TMessageRestriction> hub,
        IEventHandler<TMessage> handler,
        IEventFilter<TMessage>[] filters,
        FilterOrdering ordering)
        where TMessage : TMessageRestriction
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler, filters, ordering);
    }

    public static IDisposable Subscribe<TMessageRestriction, TMessage>(
        this IEventHub<TMessageRestriction> hub,
        IEventHandler<TMessage> handler,
        params IEventFilter<TMessage>[] filters)
        where TMessage : TMessageRestriction
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler, filters);
    }

    public static IDisposable Subscribe<TMessageRestriction, TMessage>(
        this IEventHub<TMessageRestriction> hub,
        Action<TMessage> handler,
        params IEventFilter<TMessage>[] filters)
        where TMessage : TMessageRestriction
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler, filters);
    }

    public static IDisposable Subscribe<TMessageRestriction, TMessage>(
        this IEventHub<TMessageRestriction> hub,
        Action<TMessage> handler,
        Func<TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters)
        where TMessage : TMessageRestriction
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(handler, predicateFilter, additionalFilters);
    }

    // ----- Stateful Subscribe Overloads -----
    public static IDisposable Subscribe<TMessageRestriction, TState, TMessage>(
        this IEventHub<TMessageRestriction> hub,
        TState state,
        Action<TState, TMessage> handler,
        params IEventFilter<TMessage>[] filters)
        where TMessage : TMessageRestriction
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(state, handler, filters);
    }

    public static IDisposable Subscribe<TMessageRestriction, TState, TMessage>(
        this IEventHub<TMessageRestriction> hub,
        TState state,
        Action<TState, TMessage> handler,
        Func<TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters)
        where TMessage : TMessageRestriction
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(state, handler, predicateFilter, additionalFilters);
    }

    public static IDisposable Subscribe<TMessageRestriction, TState, TMessage>(
        this IEventHub<TMessageRestriction> hub,
        TState state,
        Action<TState, TMessage> handler,
        Func<TState, TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters)
        where TMessage : TMessageRestriction
    {
        var ev = hub.GetEvent<TMessage>()
                 ?? throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the hub.");
        return ev.Subscribe(state, handler, predicateFilter, additionalFilters);
    }
}