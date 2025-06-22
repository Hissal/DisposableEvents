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
    public static IDisposable Subscribe(this IEvent e, Action action, Action<Exception>? onError = null, Action? onComplete = null) =>
        e.Subscribe(new EventReceiver(action, onError, onComplete));

    public static void Publish(this IEvent e) {
        if (e is IEvent<EmptyEvent> emptyEvent)
            emptyEvent.Publish(default);
        else
            throw new InvalidOperationException("Cannot publish to non-empty event.");
    }

    /// <summary>
    /// Subscribes to the event with an action to handle published values, and optional handlers for exceptions and event disposal.
    /// </summary>
    /// <param name="e">The event to subscribe to.</param>
    /// <param name="action">The action to invoke when a value is published.</param>
    /// <param name="filters">The filters to attach to this subscription.</param>
    /// <typeparam name="T">The type of the event data.</typeparam>
    /// <returns>A disposable subscription that can be used to unsubscribe from the event.</returns>
    public static IDisposable Subscribe<T>(this IEvent<T> e, Action<T> action, params IEventFilter<T>[] filters) {
        return e.Subscribe(new EventReceiver<T>(action), filters);
    }
    
    /// <summary>
    /// Subscribes to the event with an action to handle published values, and optional handlers for exceptions and event disposal.
    /// </summary>
    /// <param name="e">The event to subscribe to.</param>
    /// <param name="action">The action to invoke when a value is published.</param>
    /// <param name="onError">Optional action to invoke if an exception occurs during event publishing.</param>
    /// <param name="onComplete">Optional action to invoke when the event is disposed or completed.</param>
    /// <param name="filters">The filters to attach to this subscription.</param>
    /// <typeparam name="T">The type of the event data.</typeparam>
    /// <returns>A disposable subscription that can be used to unsubscribe from the event.</returns>
    public static IDisposable Subscribe<T>(this IEvent<T> e, Action<T> action, Action<Exception>? onError = null, Action? onComplete = null, params IEventFilter<T>[] filters) => 
        e.Subscribe(new EventReceiver<T>(action, onError, onComplete), filters);
}