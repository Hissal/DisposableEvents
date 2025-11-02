namespace DisposableEvents;

public static class FuncSubscriberExtensions {
    // NOTE: This overload is just for convenience, allowing for use of the default implementation in the interface from any implementation.
    public static IDisposable RegisterHandler<TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber,
        IFuncHandler<TArg, TResult> handler,
        IEventFilter<TArg> filter) 
    {
        return subscriber.RegisterHandler(handler, filter);
    }
    
    // NOTE: This overload is just for convenience, allowing for use of the default implementation in the interface from any implementation.
    public static IDisposable RegisterHandler<TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber,
        IFuncHandler<TArg, TResult> handler,
        IEventFilter<TArg>[] filters,
        FilterOrdering ordering)
    {
        return subscriber.RegisterHandler(handler, filters, ordering);
    }
    
    public static IDisposable RegisterHandler<TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber,
        IFuncHandler<TArg, TResult> handler, 
        params IEventFilter<TArg>[] filters)
    {
        return filters.Length switch {
            0 => subscriber.RegisterHandler(handler),
            1 => subscriber.RegisterHandler(handler, filters[0]),
            _ => subscriber.RegisterHandler(handler, filters, FilterOrdering.StableSort)
        };
    }
    
    public static IDisposable RegisterHandler<TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber, 
        Func<TArg, FuncResult<TResult>> handler,
        params IEventFilter<TArg>[] filters) 
    {
        return subscriber.RegisterHandler(new FuncHandler<TArg, TResult>(handler), filters);
    }

    public static IDisposable RegisterHandler<TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber,
        Func<TArg, FuncResult<TResult>> handler, 
        Func<TArg, bool> predicateFilter,
        params IEventFilter<TArg>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.RegisterHandler(
                new FuncHandler<TArg, TResult>(handler), 
                new PredicateEventFilter<TArg>(predicateFilter)
            );
        
        var filters = new IEventFilter<TArg>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TArg>(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.RegisterHandler(new FuncHandler<TArg, TResult>(handler), filters);
    }
    
    // ----- Stateful Overloads -----
    public static IDisposable RegisterHandler<TState, TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber, 
        TState state,
        Func<TState, TArg, FuncResult<TResult>> handler,
        params IEventFilter<TArg>[] filters) 
    {
        return subscriber.RegisterHandler(new FuncHandler<TState, TArg, TResult>(state, handler), filters);
    }
    
    public static IDisposable RegisterHandler<TState, TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber,
        TState state,
        Func<TState, TArg, FuncResult<TResult>> handler, 
        Func<TArg, bool> predicateFilter,
        params IEventFilter<TArg>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.RegisterHandler(
                new FuncHandler<TState, TArg, TResult>(state, handler), 
                new PredicateEventFilter<TArg>(predicateFilter)
            );
        
        var filters = new IEventFilter<TArg>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TArg>(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.RegisterHandler(new FuncHandler<TState, TArg, TResult>(state, handler), filters);
    }
    
    public static IDisposable RegisterHandler<TState, TArg, TResult>(
        this IFuncSubscriber<TArg, TResult> subscriber,
        TState state,
        Func<TState, TArg, FuncResult<TResult>> handler, 
        Func<TState, TArg, bool> predicateFilter,
        params IEventFilter<TArg>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.RegisterHandler(
                new FuncHandler<TState, TArg, TResult>(state, handler), 
                new PredicateEventFilter<TState, TArg>(state, predicateFilter)
            );
        
        var filters = new IEventFilter<TArg>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TState, TArg>(state, predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.RegisterHandler(new FuncHandler<TState, TArg, TResult>(state, handler), filters);
    }
}