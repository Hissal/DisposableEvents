namespace DisposableEvents;

public static class VoidFuncExtensions {
    public static FuncResult<TResult> Invoke<TResult>(this IFuncPublisher<Void, TResult> publisher) => publisher.Invoke(Void.Value);
    
    // ----- Subscriber Extensions -----
    public static IDisposable RegisterHandler<TResult>(
        this IFuncSubscriber<Void, TResult> subscriber, 
        Func<FuncResult<TResult>> handler,
        params IEventFilter<Void>[] filters) 
    {
        return subscriber.RegisterHandler(new VoidFuncHandler<TResult>(handler), filters);
    }
    
    public static IDisposable RegisterHandler<TResult>(
        this IFuncSubscriber<Void, TResult> subscriber,
        Func<FuncResult<TResult>> handler,
        Func<bool> predicateFilter,
        params IEventFilter<Void>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.RegisterHandler(
                new VoidFuncHandler<TResult>(handler), 
                new VoidPredicateEventFilter(predicateFilter)
            );
        
        var filters = new IEventFilter<Void>[additionalFilters.Length + 1];
        filters[0] = new VoidPredicateEventFilter(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.RegisterHandler(new VoidFuncHandler<TResult>(handler), filters);
    }
    
    // ----- Stateful Overloads -----
    public static IDisposable RegisterHandler<TState, TResult>(
        this IFuncSubscriber<Void, TResult> subscriber,
        TState state,
        Func<TState, FuncResult<TResult>> handler,
        params IEventFilter<Void>[] filters) 
    {
        return subscriber.RegisterHandler(new VoidFuncHandler<TState, TResult>(state, handler), filters);
    }
    
    public static IDisposable RegisterHandler<TState, TResult>(
        this IFuncSubscriber<Void, TResult> subscriber,
        TState state,
        Func<TState, FuncResult<TResult>> handler,
        Func<bool> predicateFilter,
        params IEventFilter<Void>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.RegisterHandler(
                new VoidFuncHandler<TState, TResult>(state, handler), 
                new VoidPredicateEventFilter(predicateFilter)
            );
        
        var filters = new IEventFilter<Void>[additionalFilters.Length + 1];
        filters[0] = new VoidPredicateEventFilter(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.RegisterHandler(new VoidFuncHandler<TState, TResult>(state, handler), filters);
    }
}