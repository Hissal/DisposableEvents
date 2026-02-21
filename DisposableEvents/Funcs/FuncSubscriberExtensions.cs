namespace DisposableEvents;

public static class FuncSubscriberExtensions {
    // NOTE: This overload is just for convenience, allowing for use of the default implementation in the interface from any implementation.
    public static IDisposable AddHandler<TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber,
        IFuncHandler<TArg, TResult> handler,
        IEventFilter<TArg> filter) 
    {
        return subscriber.AddHandler(handler, filter);
    }
    
    // NOTE: This overload is just for convenience, allowing for use of the default implementation in the interface from any implementation.
    public static IDisposable AddHandler<TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber,
        IFuncHandler<TArg, TResult> handler,
        IEventFilter<TArg>[] filters,
        FilterOrdering ordering)
    {
        return subscriber.AddHandler(handler, filters, ordering);
    }
    
    public static IDisposable AddHandler<TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber,
        IFuncHandler<TArg, TResult> handler, 
        params IEventFilter<TArg>[] filters)
    {
        return filters.Length switch {
            0 => subscriber.AddHandler(handler),
            1 => subscriber.AddHandler(handler, filters[0]),
            _ => subscriber.AddHandler(handler, filters, FilterOrdering.StableSort)
        };
    }
    
    public static IDisposable AddHandler<TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber, 
        Func<TArg, FuncResult<TResult>> handler,
        params IEventFilter<TArg>[] filters) 
    {
        return subscriber.AddHandler(new FuncHandler<TArg, TResult>(handler), filters);
    }

    public static IDisposable AddHandler<TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber,
        Func<TArg, FuncResult<TResult>> handler, 
        Func<TArg, bool> predicateFilter,
        params IEventFilter<TArg>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.AddHandler(
                new FuncHandler<TArg, TResult>(handler), 
                new PredicateEventFilter<TArg>(predicateFilter)
            );
        
        var filters = new IEventFilter<TArg>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TArg>(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.AddHandler(new FuncHandler<TArg, TResult>(handler), filters);
    }
    
    // ----- Stateful Overloads -----
    public static IDisposable AddHandler<TState, TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber, 
        TState state,
        Func<TState, TArg, FuncResult<TResult>> handler,
        params IEventFilter<TArg>[] filters) 
    {
        return subscriber.AddHandler(new FuncHandler<TState, TArg, TResult>(state, handler), filters);
    }
    
    public static IDisposable AddHandler<TState, TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber,
        TState state,
        Func<TState, TArg, FuncResult<TResult>> handler, 
        Func<TArg, bool> predicateFilter,
        params IEventFilter<TArg>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.AddHandler(
                new FuncHandler<TState, TArg, TResult>(state, handler), 
                new PredicateEventFilter<TArg>(predicateFilter)
            );
        
        var filters = new IEventFilter<TArg>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TArg>(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.AddHandler(new FuncHandler<TState, TArg, TResult>(state, handler), filters);
    }
    
    public static IDisposable AddHandler<TState, TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber,
        TState state,
        Func<TState, TArg, FuncResult<TResult>> handler, 
        Func<TState, TArg, bool> predicateFilter,
        params IEventFilter<TArg>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.AddHandler(
                new FuncHandler<TState, TArg, TResult>(state, handler), 
                new PredicateEventFilter<TState, TArg>(state, predicateFilter)
            );
        
        var filters = new IEventFilter<TArg>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TState, TArg>(state, predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.AddHandler(new FuncHandler<TState, TArg, TResult>(state, handler), filters);
    }
}