namespace DisposableEvents;

public sealed class OneShotEventHandler<TMessage> : IEventHandler<TMessage> {
    readonly IEventHandler<TMessage> innerHandler;
    IDisposable? sub;
    int invoked = 0; // 0 = not invoked, 1 = invoked
    
    public OneShotEventHandler(IEventHandler<TMessage> innerHandler) {
        this.innerHandler = innerHandler;
    }

    public void SetSubscription(IDisposable subscription) {
        // Atomically set the subscription only if it hasn't been set yet
        var previous = Interlocked.CompareExchange(ref sub, subscription, null);
        if (previous != null) {
            // Subscription was already set somehow, dispose the new one
            subscription.Dispose();
            return;
        }
        
        // Check if Handle was already called before we set the subscription
        if (Volatile.Read(ref invoked) == 1) {
            // Handler already invoked. Try to atomically reclaim the subscription we just set.
            // CompareExchange will only succeed if sub still equals subscription (we haven't been
            // raced by Handle's Exchange). If Handle already claimed it, this returns null.
            var toDispose = Interlocked.CompareExchange(ref sub, null, subscription);
            // Only dispose if we successfully reclaimed it, ensuring no double-dispose
            if (toDispose == subscription) {
                subscription.Dispose();
            }
        }
    }
    
    public void Handle(TMessage message) {
        if (Interlocked.Exchange(ref invoked, 1) == 1)
            return; // already invoked
        
        innerHandler.Handle(message);
        // Atomically take ownership of the subscription before disposing
        var toDispose = Interlocked.Exchange(ref sub, null);
        toDispose?.Dispose();
    }
}

public static class SubscribeOnceExtensions {
    public static IDisposable SubscribeOnce<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        IEventHandler<TMessage> handler,
        IEventFilter<TMessage> filter) 
    {
        var oneShotHandler = new OneShotEventHandler<TMessage>(handler);
        var subscription = subscriber.Subscribe(oneShotHandler, filter);
        oneShotHandler.SetSubscription(subscription);
        return subscription;
    }
    
    public static IDisposable SubscribeOnce<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        IEventHandler<TMessage> handler, 
        IEventFilter<TMessage>[] filters, 
        FilterOrdering ordering) 
    {
        var oneShotHandler = new OneShotEventHandler<TMessage>(handler);
        var subscription = subscriber.Subscribe(oneShotHandler, filters, ordering);
        oneShotHandler.SetSubscription(subscription);
        return subscription;
    }
    
    public static IDisposable SubscribeOnce<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        IEventHandler<TMessage> handler,
        params IEventFilter<TMessage>[] filters)
    {
        var oneShotHandler = new OneShotEventHandler<TMessage>(handler);
        var subscription = subscriber.Subscribe(oneShotHandler, filters);
        oneShotHandler.SetSubscription(subscription);
        return subscription;
    }
    
    // ===== Action Overloads ===== //
    public static IDisposable SubscribeOnce<TMessage>(
        this IEventSubscriber<TMessage> subscriber, 
        Action<TMessage> handler,
        params IEventFilter<TMessage>[] filters) 
    {
        var eventHandler = new EventHandler<TMessage>(handler);
        return subscriber.SubscribeOnce(eventHandler, filters);
    }

    public static IDisposable SubscribeOnce<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        Action<TMessage> handler, 
        Func<TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.SubscribeOnce(
                new EventHandler<TMessage>(handler), 
                new PredicateEventFilter<TMessage>(predicateFilter)
            );
        
        var filters = new IEventFilter<TMessage>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TMessage>(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        
        var eventHandler = new EventHandler<TMessage>(handler);
        return subscriber.SubscribeOnce(eventHandler, filters);
    }
    
    // ----- Stateful Overloads ----- //
    public static IDisposable SubscribeOnce<TState, TMessage>(
        this IEventSubscriber<TMessage> subscriber, 
        TState state,
        Action<TState, TMessage> handler,
        params IEventFilter<TMessage>[] filters) 
    {
        var eventHandler = new EventHandler<TState, TMessage>(state, handler);
        return subscriber.SubscribeOnce(eventHandler, filters);
    }
    
    public static IDisposable SubscribeOnce<TState, TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        TState state,
        Action<TState, TMessage> handler, 
        Func<TState, TMessage, bool> predicateFilter,
        params IEventFilter<TMessage>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.SubscribeOnce(
                new EventHandler<TState, TMessage>(state, handler), 
                new PredicateEventFilter<TState, TMessage>(state, predicateFilter)
            );
        
        var filters = new IEventFilter<TMessage>[additionalFilters.Length + 1];
        filters[0] = new PredicateEventFilter<TState, TMessage>(state, predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        
        var eventHandler = new EventHandler<TState, TMessage>(state, handler);
        return subscriber.SubscribeOnce(eventHandler, filters);
    }
    
    // -------------------------- //
    // ===== Void Overloads ===== //
    // -------------------------- //
    
    public static IDisposable SubscribeOnce(
        this IEventSubscriber<Void> subscriber, 
        Action handler,
        params IEventFilter<Void>[] filters) 
    {
        return subscriber.SubscribeOnce( new VoidEventHandler(handler), filters);
    }
    
    public static IDisposable SubscribeOnce(
        this IEventSubscriber<Void> subscriber,
        Action handler,
        Func<bool> predicateFilter,
        params IEventFilter<Void>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.SubscribeOnce(
                new VoidEventHandler(handler), 
                new VoidPredicateEventFilter(predicateFilter)
            );
        
        var filters = new IEventFilter<Void>[additionalFilters.Length + 1];
        filters[0] = new VoidPredicateEventFilter(predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.SubscribeOnce(new VoidEventHandler(handler), filters);
    }
    
    // ----- Stateful Overloads ----- //
    public static IDisposable SubscribeOnce<TState>(
        this IEventSubscriber<Void> subscriber, 
        TState state,
        Action<TState> handler,
        params IEventFilter<Void>[] filters) 
    {
        return subscriber.SubscribeOnce(new VoidEventHandler<TState>(state, handler), filters);
    }
    
    public static IDisposable SubscribeOnce<TState>(
        this IEventSubscriber<Void> subscriber,
        TState state,
        Action<TState> handler,
        Func<TState, bool> predicateFilter,
        params IEventFilter<Void>[] additionalFilters) 
    {
        if (additionalFilters.Length == 0)
            return subscriber.SubscribeOnce(
                new VoidEventHandler<TState>(state, handler), 
                new VoidPredicateEventFilter<TState>(state, predicateFilter)
            );
        
        var filters = new IEventFilter<Void>[additionalFilters.Length + 1];
        filters[0] = new VoidPredicateEventFilter<TState>(state, predicateFilter);
        Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
        return subscriber.SubscribeOnce(new VoidEventHandler<TState>(state, handler), filters);
    }
}