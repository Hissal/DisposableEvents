namespace DisposableEvents;

public static class EventExtensions {
    /// <summary>
    /// Subscribes to the event with an action to handle published values, and optional handlers for exceptions and event disposal.
    /// </summary>
    /// <param name="e">The event to subscribe to.</param>
    /// <param name="action">The action to invoke when a value is published.</param>
    /// <param name="onError">Optional action to invoke if an exception occurs during event publishing.</param>
    /// <param name="onComplete">Optional action to invoke when the event is disposed or completed.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe from the event.</returns>
    public static IDisposable Subscribe(this ISubscriber<EmptyEvent> e, Action action, Action<Exception>? onError = null, Action? onComplete = null) =>
        e.Subscribe(new EventObserver(action, onError, onComplete));

    public static void Publish(this IPublisher<EmptyEvent> e) => 
        e.Publish(default);

    /// <summary>
    /// Subscribes to the event with an action to handle published values, and optional handlers for exceptions and event disposal.
    /// </summary>
    /// <param name="subscriber">The event to subscribe to.</param>
    /// <param name="action">The action to invoke when a value is published.</param>
    /// <param name="filters">The filters to attach to this subscription.</param>
    /// <typeparam name="TMessage">The type of the event data.</typeparam>
    /// <returns>A disposable subscription that can be used to unsubscribe from the event.</returns>
    public static IDisposable Subscribe<TMessage>(this ISubscriber<TMessage> subscriber, Action<TMessage> action, params IEventFilter<TMessage>[] filters) => 
        subscriber.Subscribe(new EventObserver<TMessage>(action), filters);
    
    public static IDisposable Subscribe<TClosure, TMessage>(this ISubscriber<TMessage> subscriber, TClosure closure, Action<TClosure, TMessage> action, params IEventFilter<TMessage>[] filters) =>
        subscriber.Subscribe(new ClosureEventObserver<TClosure,TMessage>(closure, action), filters);

    /// <summary>
    /// Subscribes to the event with an action to handle published values, and optional handlers for exceptions and event disposal.
    /// </summary>
    /// <param name="e">The event to subscribe to.</param>
    /// <param name="action">The action to invoke when a value is published.</param>
    /// <param name="onError">Optional action to invoke if an exception occurs during event publishing.</param>
    /// <param name="onComplete">Optional action to invoke when the event is disposed or completed.</param>
    /// <param name="filters">The filters to attach to this subscription.</param>
    /// <typeparam name="TMessage">The type of the event data.</typeparam>
    /// <returns>A disposable subscription that can be used to unsubscribe from the event.</returns>
    public static IDisposable Subscribe<TMessage>(this ISubscriber<TMessage> e, Action<TMessage> action, Action<Exception>? onError = null, Action? onComplete = null, params IEventFilter<TMessage>[] filters) => 
        e.Subscribe(new EventObserver<TMessage>(action, onError, onComplete), filters);
    
    
    /// <summary>
    /// Subscribes to the event with an action to handle published values, using a predicate filter to determine which values are delivered.
    /// </summary>
    /// <param name="e">The event to subscribe to.</param>
    /// <param name="action">The action to invoke when a value is published and passes the filter.</param>
    /// <param name="filter">A predicate that returns true to allow the value through, or false to filter it out.</param>
    /// <typeparam name="TMessage">The type of the event data.</typeparam>
    /// <returns>A disposable subscription that can be used to unsubscribe from the event.</returns>
    public static IDisposable Subscribe<TMessage>(this ISubscriber<TMessage> e, Action<TMessage> action, Func<TMessage, bool> filter) => 
        e.Subscribe(new EventObserver<TMessage>(action), new PredicateEventFilter<TMessage>(filter));

    /// <summary>
    /// Subscribes to the event with an action to handle published values, using predicate filters for event, error, and completion.
    /// </summary>
    /// <param name="e">The event to subscribe to.</param>
    /// <param name="action">The action to invoke when a value is published and passes the event filter.</param>
    /// <param name="eventFilter">Predicate to filter event values. Returns true to allow, false to filter out.</param>
    /// <param name="errorFilter">Predicate to filter errors. Returns true to allow, false to filter out.</param>
    /// <param name="completedFilter">Predicate to filter completion. Returns true to allow, false to filter out.</param>
    /// <typeparam name="TMessage">The type of the event data.</typeparam>
    /// <returns>A disposable subscription that can be used to unsubscribe from the event.</returns>
    public static IDisposable Subscribe<TMessage>(this ISubscriber<TMessage> e, Action<TMessage> action, Func<TMessage, bool> eventFilter, Func<Exception, bool> errorFilter, Func<bool> completedFilter) => 
        e.Subscribe(new EventObserver<TMessage>(action), new PredicateEventFilter<TMessage>(eventFilter, errorFilter, completedFilter));
}