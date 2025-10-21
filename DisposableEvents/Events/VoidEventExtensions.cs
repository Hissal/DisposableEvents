namespace DisposableEvents;

public static class VoidEventExtensions {
    public static void Publish(this IEventPublisher<Void> publisher) => publisher.Publish(Void.Value);
    
    // ----- Subscriber Extensions -----
    public static IDisposable Subscribe(
        this IEventSubscriber<Void> subscriber, 
        Action handler,
        params IEventFilter<Void>[] filters) 
    {
        return subscriber.Subscribe(new VoidEventHandler(handler), filters);
    }
    
    public static IDisposable Subscribe(
        this IEventSubscriber<Void> subscriber,
        Action handler,
        Func<bool> predicateFilter,
        params IEventFilter<Void>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.Subscribe(
                new VoidEventHandler(handler), 
                new VoidPredicateEventFilter(predicateFilter)
            );
        
        var filters = new IEventFilter<Void>[additionalFilters.Length + 1];
        filters[0] = new VoidPredicateEventFilter(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.Subscribe(new VoidEventHandler(handler), filters);
    }
    
    // ----- Stateful Overloads -----
    public static IDisposable Subscribe<TState>(
        this IEventSubscriber<Void> subscriber, 
        TState state,
        Action<TState> handler,
        params IEventFilter<Void>[] filters) 
    {
        return subscriber.Subscribe(new VoidEventHandler<TState>(state, handler), filters);
    }
    
    public static IDisposable Subscribe<TState>(
        this IEventSubscriber<Void> subscriber,
        TState state,
        Action<TState> handler,
        Func<bool> predicateFilter,
        params IEventFilter<Void>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.Subscribe(
                new VoidEventHandler<TState>(state, handler), 
                new VoidPredicateEventFilter(predicateFilter)
            );
        
        var filters = new IEventFilter<Void>[additionalFilters.Length + 1];
        filters[0] = new VoidPredicateEventFilter(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.Subscribe(new VoidEventHandler<TState>(state, handler), filters);
    }
}