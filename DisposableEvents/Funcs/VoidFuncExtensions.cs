using Void = HCommons.Void.Void;

namespace DisposableEvents;

public static class VoidFuncExtensions {
    public static FuncResult<TResult> Invoke<TResult>(this IFuncPublisher<Void, TResult> publisher) => publisher.Invoke(Void.Value);
    
    // ----- Subscriber Extensions -----
    public static IDisposable AddHandler<TResult>(
        this IFuncSubscriber<Void, TResult> subscriber, 
        Func<FuncResult<TResult>> handler,
        params IEventFilter<Void>[] filters) 
    {
        return subscriber.AddHandler(new VoidFuncHandler<TResult>(handler), filters);
    }
    
    public static IDisposable AddHandler<TResult>(
        this IFuncSubscriber<Void, TResult> subscriber,
        Func<FuncResult<TResult>> handler,
        Func<bool> predicateFilter,
        params IEventFilter<Void>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.AddHandler(
                new VoidFuncHandler<TResult>(handler), 
                new VoidPredicateEventFilter(predicateFilter)
            );
        
        var filters = new IEventFilter<Void>[additionalFilters.Length + 1];
        filters[0] = new VoidPredicateEventFilter(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.AddHandler(new VoidFuncHandler<TResult>(handler), filters);
    }
    
    // ----- Stateful Overloads -----
    public static IDisposable AddHandler<TState, TResult>(
        this IFuncSubscriber<Void, TResult> subscriber,
        TState state,
        Func<TState, FuncResult<TResult>> handler,
        params IEventFilter<Void>[] filters) 
    {
        return subscriber.AddHandler(new VoidFuncHandler<TState, TResult>(state, handler), filters);
    }
    
    public static IDisposable AddHandler<TState, TResult>(
        this IFuncSubscriber<Void, TResult> subscriber,
        TState state,
        Func<TState, FuncResult<TResult>> handler,
        Func<bool> predicateFilter,
        params IEventFilter<Void>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.AddHandler(
                new VoidFuncHandler<TState, TResult>(state, handler), 
                new VoidPredicateEventFilter(predicateFilter)
            );
        
        var filters = new IEventFilter<Void>[additionalFilters.Length + 1];
        filters[0] = new VoidPredicateEventFilter(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.AddHandler(new VoidFuncHandler<TState, TResult>(state, handler), filters);
    }
}