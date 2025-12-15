namespace DisposableEvents;

/// <summary>
/// Marker Interface for events to allow for non-generic storage and disposal.
/// </summary>
public interface IEventMarker : IDisposable;

public interface IEventPublisher<TMessage> : IDisposable {
    public bool IsDisposed { get; }
    public int HandlerCount { get; }
    
    /// <summary>
    /// Publishes a value to all subscribed observers.
    /// </summary>
    /// <param name="message">The value to publish.</param>
    void Publish(TMessage message);
    
    EventHandlerSnapshot<TMessage> SnapshotHandlers();
    void ClearHandlers();
  
    // Would require synchronization to implement properly
    // Maybe in the future with a publicly available sync root
    // ReadOnlySpan<IEventHandler<TMessage>> GetHandlersSpan();
    // IEventHandler<TMessage>[] GetHandlers();
}

public interface IEventSubscriber<TMessage> {
    IDisposable Subscribe(IEventHandler<TMessage> handler);

    IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter)
#if NETSTANDARD2_0
        ;
#else
    {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, filter);
        return Subscribe(filteredHandler);
    }
#endif

    IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering)
#if NETSTANDARD2_0
        ;
#else
    {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, filters, ordering);
        return Subscribe(filteredHandler);
    }
#endif
}

public interface IDisposableEvent<TMessage> : IEventPublisher<TMessage>, IEventSubscriber<TMessage>, IEventMarker;