namespace DisposableEvents;

public static class VoidFuncExtensions {
    public static FuncResult<TReturn> Invoke<TReturn>(this IFuncPublisher<Void, TReturn> publisher) => publisher.Invoke(Void.Value);
    
    // ----- Subscriber Extensions -----
    public static IDisposable RegisterCallback<TReturn>(
        this IFuncSubscriber<Void, TReturn> subscriber, 
        Func<FuncResult<TReturn>> handler,
        params IEventFilter<Void>[] filters) 
    {
        return subscriber.RegisterCallback(new VoidFuncHandler<TReturn>(handler), filters);
    }
    
    public static IDisposable RegisterCallback<TReturn>(
        this IFuncSubscriber<Void, TReturn> subscriber,
        Func<FuncResult<TReturn>> handler,
        Func<bool> predicateFilter,
        params IEventFilter<Void>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.RegisterCallback(
                new VoidFuncHandler<TReturn>(handler), 
                new VoidPredicateEventFilter(predicateFilter)
            );
        
        var filters = new IEventFilter<Void>[additionalFilters.Length + 1];
        filters[0] = new VoidPredicateEventFilter(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.RegisterCallback(new VoidFuncHandler<TReturn>(handler), filters);
    }
    
    // ----- Stateful Overloads -----
    public static IDisposable RegisterCallback<TState, TReturn>(
        this IFuncSubscriber<Void, TReturn> subscriber,
        TState state,
        Func<TState, FuncResult<TReturn>> handler,
        params IEventFilter<Void>[] filters) 
    {
        return subscriber.RegisterCallback(new StatefulVoidFuncHandler<TState, TReturn>(state, handler), filters);
    }
    
    public static IDisposable RegisterCallback<TState, TReturn>(
        this IFuncSubscriber<Void, TReturn> subscriber,
        TState state,
        Func<TState, FuncResult<TReturn>> handler,
        Func<bool> predicateFilter,
        params IEventFilter<Void>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.RegisterCallback(
                new StatefulVoidFuncHandler<TState, TReturn>(state, handler), 
                new VoidPredicateEventFilter(predicateFilter)
            );
        
        var filters = new IEventFilter<Void>[additionalFilters.Length + 1];
        filters[0] = new VoidPredicateEventFilter(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.RegisterCallback(new StatefulVoidFuncHandler<TState, TReturn>(state, handler), filters);
    }
}