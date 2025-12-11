namespace DisposableEvents;

/// <summary>
/// An event handler that invokes the inner handler at most once, then disposes its subscription.
/// Thread-safe: ensures the handler is invoked only once even in the presence of concurrent calls.
/// </summary>
/// <typeparam name="TMessage">The type of message handled by this event handler.</typeparam>
public sealed class OneShotEventHandler<TMessage> : IEventHandler<TMessage> {
    readonly IEventHandler<TMessage> innerHandler;
    IDisposable? sub;
    int invoked = 0; // 0 = not invoked, 1 = invoked
    
    public OneShotEventHandler(IEventHandler<TMessage> innerHandler) {
        this.innerHandler = innerHandler;
    }

    /// <summary>
    /// Sets the subscription that will be disposed after the first event invocation.
    /// If the handler has already been invoked before this method is called, the subscription is disposed immediately.
    /// </summary>
    /// <param name="subscription">The subscription to dispose after the first event.</param>
    public void SetSubscription(IDisposable subscription) {
        // Atomically set the subscription only if it hasn't been set yet
        var previous = Interlocked.CompareExchange(ref sub, subscription, null);
        if (previous != null) {
            // Subscription was already set somehow, dispose the new one
            subscription.Dispose();
            return;
        }
        
        // Check if Handle was already called before we set the subscription
        if (Interlocked.CompareExchange(ref invoked, 0, 0) == 1) {
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
    
    /// <summary>
    /// Handles the event message. The first call invokes the inner handler and disposes the subscription.
    /// Subsequent calls are ignored.
    /// </summary>
    /// <param name="message">The event message to handle.</param>
    public void Handle(TMessage message) {
        if (Interlocked.Exchange(ref invoked, 1) == 1)
            return; // already invoked

        try {
            innerHandler.Handle(message);
        }
        finally {
            // Atomically take ownership of the subscription before disposing
            var toDispose = Interlocked.Exchange(ref sub, null);
            toDispose?.Dispose();
        }
    }
}

/// <summary>
/// Provides extension methods for subscribing to events with automatic unsubscription after the first matching event.
/// </summary>
public static class SubscribeOnceExtensions {
    /// <summary>
    /// Subscribes to an event with a handler that will be invoked at most once and then automatically unsubscribed.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to subscribe to.</typeparam>
    /// <param name="subscriber">The event subscriber to subscribe to.</param>
    /// <param name="handler">The event handler to invoke once.</param>
    /// <param name="filter">The filter to apply to events.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe before the first event.</returns>
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
    
    /// <summary>
    /// Subscribes to an event with a handler that will be invoked at most once and then automatically unsubscribed.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to subscribe to.</typeparam>
    /// <param name="subscriber">The event subscriber to subscribe to.</param>
    /// <param name="handler">The event handler to invoke once.</param>
    /// <param name="filters">The filters to apply to events.</param>
    /// <param name="ordering">The ordering strategy for applying multiple filters.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe before the first event.</returns>
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
    
    /// <summary>
    /// Subscribes to an event with a handler that will be invoked at most once and then automatically unsubscribed.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to subscribe to.</typeparam>
    /// <param name="subscriber">The event subscriber to subscribe to.</param>
    /// <param name="handler">The event handler to invoke once.</param>
    /// <param name="filters">Optional filters to apply to events.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe before the first event.</returns>
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
    /// <summary>
    /// Subscribes to an event with an action that will be invoked at most once and then automatically unsubscribed.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to subscribe to.</typeparam>
    /// <param name="subscriber">The event subscriber to subscribe to.</param>
    /// <param name="handler">The action to invoke once when an event occurs.</param>
    /// <param name="filters">Optional filters to apply to events.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe before the first event.</returns>
    public static IDisposable SubscribeOnce<TMessage>(
        this IEventSubscriber<TMessage> subscriber, 
        Action<TMessage> handler,
        params IEventFilter<TMessage>[] filters) 
    {
        var eventHandler = new EventHandler<TMessage>(handler);
        return subscriber.SubscribeOnce(eventHandler, filters);
    }

    /// <summary>
    /// Subscribes to an event with an action that will be invoked at most once and then automatically unsubscribed.
    /// Includes a predicate filter for event filtering.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to subscribe to.</typeparam>
    /// <param name="subscriber">The event subscriber to subscribe to.</param>
    /// <param name="handler">The action to invoke once when an event occurs.</param>
    /// <param name="predicateFilter">A predicate to filter events before invoking the handler.</param>
    /// <param name="additionalFilters">Optional additional filters to apply to events.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe before the first event.</returns>
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
    /// <summary>
    /// Subscribes to an event with a stateful action that will be invoked at most once and then automatically unsubscribed.
    /// </summary>
    /// <typeparam name="TState">The type of state to pass to the handler.</typeparam>
    /// <typeparam name="TMessage">The type of message to subscribe to.</typeparam>
    /// <param name="subscriber">The event subscriber to subscribe to.</param>
    /// <param name="state">The state to pass to the handler when invoked.</param>
    /// <param name="handler">The stateful action to invoke once when an event occurs.</param>
    /// <param name="filters">Optional filters to apply to events.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe before the first event.</returns>
    public static IDisposable SubscribeOnce<TState, TMessage>(
        this IEventSubscriber<TMessage> subscriber, 
        TState state,
        Action<TState, TMessage> handler,
        params IEventFilter<TMessage>[] filters) 
    {
        var eventHandler = new EventHandler<TState, TMessage>(state, handler);
        return subscriber.SubscribeOnce(eventHandler, filters);
    }
    
    /// <summary>
    /// Subscribes to an event with a stateful action that will be invoked at most once and then automatically unsubscribed.
    /// Includes a stateful predicate filter for event filtering.
    /// </summary>
    /// <typeparam name="TState">The type of state to pass to the handler and filter.</typeparam>
    /// <typeparam name="TMessage">The type of message to subscribe to.</typeparam>
    /// <param name="subscriber">The event subscriber to subscribe to.</param>
    /// <param name="state">The state to pass to the handler and filter when invoked.</param>
    /// <param name="handler">The stateful action to invoke once when an event occurs.</param>
    /// <param name="predicateFilter">A stateful predicate to filter events before invoking the handler.</param>
    /// <param name="additionalFilters">Optional additional filters to apply to events.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe before the first event.</returns>
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
    
    /// <summary>
    /// Subscribes to a void event with an action that will be invoked at most once and then automatically unsubscribed.
    /// </summary>
    /// <param name="subscriber">The event subscriber to subscribe to.</param>
    /// <param name="handler">The action to invoke once when an event occurs.</param>
    /// <param name="filters">Optional filters to apply to events.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe before the first event.</returns>
    public static IDisposable SubscribeOnce(
        this IEventSubscriber<Void> subscriber, 
        Action handler,
        params IEventFilter<Void>[] filters) 
    {
        return subscriber.SubscribeOnce( new VoidEventHandler(handler), filters);
    }
    
    /// <summary>
    /// Subscribes to a void event with an action that will be invoked at most once and then automatically unsubscribed.
    /// Includes a predicate filter for event filtering.
    /// </summary>
    /// <param name="subscriber">The event subscriber to subscribe to.</param>
    /// <param name="handler">The action to invoke once when an event occurs.</param>
    /// <param name="predicateFilter">A predicate to filter events before invoking the handler.</param>
    /// <param name="additionalFilters">Optional additional filters to apply to events.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe before the first event.</returns>
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
    /// <summary>
    /// Subscribes to a void event with a stateful action that will be invoked at most once and then automatically unsubscribed.
    /// </summary>
    /// <typeparam name="TState">The type of state to pass to the handler.</typeparam>
    /// <param name="subscriber">The event subscriber to subscribe to.</param>
    /// <param name="state">The state to pass to the handler when invoked.</param>
    /// <param name="handler">The stateful action to invoke once when an event occurs.</param>
    /// <param name="filters">Optional filters to apply to events.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe before the first event.</returns>
    public static IDisposable SubscribeOnce<TState>(
        this IEventSubscriber<Void> subscriber, 
        TState state,
        Action<TState> handler,
        params IEventFilter<Void>[] filters) 
    {
        return subscriber.SubscribeOnce(new VoidEventHandler<TState>(state, handler), filters);
    }
    
    /// <summary>
    /// Subscribes to a void event with a stateful action that will be invoked at most once and then automatically unsubscribed.
    /// Includes a stateful predicate filter for event filtering.
    /// </summary>
    /// <typeparam name="TState">The type of state to pass to the handler and filter.</typeparam>
    /// <param name="subscriber">The event subscriber to subscribe to.</param>
    /// <param name="state">The state to pass to the handler and filter when invoked.</param>
    /// <param name="handler">The stateful action to invoke once when an event occurs.</param>
    /// <param name="predicateFilter">A stateful predicate to filter events before invoking the handler.</param>
    /// <param name="additionalFilters">Optional additional filters to apply to events.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe before the first event.</returns>
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