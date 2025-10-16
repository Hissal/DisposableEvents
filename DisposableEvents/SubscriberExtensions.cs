namespace DisposableEvents;

/// <summary>
/// Provides extension methods for subscribing to and publishing events with various observer and filter configurations.
/// </summary>
public static class SubscriberExtensions {
    // NOTE: This overload is just for convenience, allowing for use of the default implementation in the interface from any implementation.
    public static IDisposable Subscribe<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        IEventHandler<TMessage> handler,
        IEventFilter<TMessage> filter) 
    {
        return subscriber.Subscribe(handler, filter);
    }
    
    // NOTE: This overload is just for convenience, allowing for use of the default implementation in the interface from any implementation.
    public static IDisposable Subscribe<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        IEventHandler<TMessage> handler, 
        IEventFilter<TMessage>[] filters, 
        FilterOrdering ordering) 
    {
        return subscriber.Subscribe(handler, filters, ordering);
    }
    
    public static IDisposable Subscribe<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        IEventHandler<TMessage> handler, 
        params IEventFilter<TMessage>[] filters)
    {
        return filters.Length switch {
            0 => subscriber.Subscribe(handler),
            1 => subscriber.Subscribe(handler, filters[0]),
            _ => subscriber.Subscribe(handler, filters, FilterOrdering.StableSort)
        };
    }
    
    public static IDisposable Subscribe<TMessage>(
        this IEventSubscriber<TMessage> subscriber, 
        Action<TMessage> handler,
        params IEventFilter<TMessage>[] filters) 
    {
        return subscriber.Subscribe(new EventHandler<TMessage>(handler), filters);
    }

    public static IDisposable Subscribe<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        Action<TMessage> handler,
        Func<TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.Subscribe(
                new EventHandler<TMessage>(handler), 
                new PredicateEventFilter<TMessage>(predicateFilter)
            );
        
        var filters = new IEventFilter<TMessage>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TMessage>(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.Subscribe(new EventHandler<TMessage>(handler), filters);
    }
    
    // ----- Stateful Overloads -----
    public static IDisposable Subscribe<TState, TMessage>(
        this IEventSubscriber<TMessage> subscriber, 
        TState state,
        Action<TState, TMessage> handler,
        params IEventFilter<TMessage>[] filters) 
    {
        return subscriber.Subscribe(new StatefulEventHandler<TState, TMessage>(state, handler), filters);
    }
    
    public static IDisposable Subscribe<TState, TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        TState state,
        Action<TState, TMessage> handler,
        Func<TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.Subscribe(
                new StatefulEventHandler<TState, TMessage>(state, handler), 
                new PredicateEventFilter<TMessage>(predicateFilter)
            );
        
        var filters = new IEventFilter<TMessage>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TMessage>(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.Subscribe(new StatefulEventHandler<TState, TMessage>(state, handler), filters);
    }
}