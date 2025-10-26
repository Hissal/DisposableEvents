namespace DisposableEvents;

public static class FuncSubscriberExtensions {
    // NOTE: This overload is just for convenience, allowing for use of the default implementation in the interface from any implementation.
    public static IDisposable RegisterCallback<TMessage, TReturn>(
        this IFuncSubscriber<TMessage, TReturn> subscriber,
        IFuncHandler<TMessage, TReturn> handler,
        IEventFilter<TMessage> filter) 
    {
        return subscriber.RegisterCallback(handler, filter);
    }
    
    // NOTE: This overload is just for convenience, allowing for use of the default implementation in the interface from any implementation.
    public static IDisposable RegisterCallback<TMessage, TReturn>(
        this IFuncSubscriber<TMessage, TReturn> subscriber,
        IFuncHandler<TMessage, TReturn> handler,
        IEventFilter<TMessage>[] filters,
        FilterOrdering ordering)
    {
        return subscriber.RegisterCallback(handler, filters, ordering);
    }
    
    public static IDisposable RegisterCallback<TMessage, TReturn>(
        this IFuncSubscriber<TMessage, TReturn> subscriber,
        IFuncHandler<TMessage, TReturn> handler, 
        params IEventFilter<TMessage>[] filters)
    {
        return filters.Length switch {
            0 => subscriber.RegisterCallback(handler),
            1 => subscriber.RegisterCallback(handler, filters[0]),
            _ => subscriber.RegisterCallback(handler, filters, FilterOrdering.StableSort)
        };
    }
    
    public static IDisposable RegisterCallback<TMessage, TReturn>(
        this IFuncSubscriber<TMessage, TReturn> subscriber, 
        Func<TMessage, FuncResult<TReturn>> handler,
        params IEventFilter<TMessage>[] filters) 
    {
        return subscriber.RegisterCallback(new FuncHandler<TMessage, TReturn>(handler), filters);
    }

    public static IDisposable RegisterCallback<TMessage, TReturn>(
        this IFuncSubscriber<TMessage, TReturn> subscriber,
        Func<TMessage, FuncResult<TReturn>> handler, 
        Func<TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.RegisterCallback(
                new FuncHandler<TMessage, TReturn>(handler), 
                new PredicateEventFilter<TMessage>(predicateFilter)
            );
        
        var filters = new IEventFilter<TMessage>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TMessage>(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.RegisterCallback(new FuncHandler<TMessage, TReturn>(handler), filters);
    }
    
    // ----- Stateful Overloads -----
    public static IDisposable RegisterCallback<TState, TMessage, TReturn>(
        this IFuncSubscriber<TMessage, TReturn> subscriber, 
        TState state,
        Func<TState, TMessage, FuncResult<TReturn>> handler,
        params IEventFilter<TMessage>[] filters) 
    {
        return subscriber.RegisterCallback(new StatefulFuncHandler<TState, TMessage, TReturn>(state, handler), filters);
    }
    
    public static IDisposable RegisterCallback<TState, TMessage, TReturn>(
        this IFuncSubscriber<TMessage, TReturn> subscriber,
        TState state,
        Func<TState, TMessage, FuncResult<TReturn>> handler, 
        Func<TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.RegisterCallback(
                new StatefulFuncHandler<TState, TMessage, TReturn>(state, handler), 
                new PredicateEventFilter<TMessage>(predicateFilter)
            );
        
        var filters = new IEventFilter<TMessage>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TMessage>(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.RegisterCallback(new StatefulFuncHandler<TState, TMessage, TReturn>(state, handler), filters);
    }
    
    public static IDisposable RegisterCallback<TState, TMessage, TReturn>(
        this IFuncSubscriber<TMessage, TReturn> subscriber,
        TState state,
        Func<TState, TMessage, FuncResult<TReturn>> handler, 
        Func<TState, TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.RegisterCallback(
                new StatefulFuncHandler<TState, TMessage, TReturn>(state, handler), 
                new PredicateEventFilter<TState, TMessage>(state, predicateFilter)
            );
        
        var filters = new IEventFilter<TMessage>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TState, TMessage>(state, predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.RegisterCallback(new StatefulFuncHandler<TState, TMessage, TReturn>(state, handler), filters);
    }
}