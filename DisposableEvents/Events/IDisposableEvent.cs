namespace DisposableEvents;

/// <summary>
/// Marker Interface for events to allow for non-generic storage and disposal.
/// </summary>
public interface IEventMarker : IDisposable;

public interface IEventPublisher<in TMessage> : IDisposable {
    public bool IsDisposed { get; }
    public int HandlerCount { get; }
    
    /// <summary>
    /// Publishes a value to all subscribed observers.
    /// </summary>
    /// <param name="message">The value to publish.</param>
    void Publish(TMessage message);
    
    IEventHandler<TMessage>[] GetHandlers();
}

public interface IEventSubscriber<TMessage> {
    IDisposable Subscribe(IEventHandler<TMessage> handler);

    IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter) {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, filter);
        return Subscribe(filteredHandler);
    }
    
    IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering) {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, filters, ordering);
        return Subscribe(filteredHandler);
    }
}

public interface IDisposableEvent<TMessage> : IEventPublisher<TMessage>, IEventSubscriber<TMessage>, IEventMarker {
    void ClearSubscriptions();
}