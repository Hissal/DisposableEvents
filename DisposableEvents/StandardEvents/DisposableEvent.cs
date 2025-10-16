
namespace DisposableEvents;

/// <summary>
/// Marker Interface for events to allow for non-generic storage and disposal.
/// </summary>
public interface IEventMarker;

public interface IEventPublisher<in TMessage> {
    /// <summary>
    /// Publishes a value to all subscribed observers.
    /// </summary>
    /// <param name="message">The value to publish.</param>
    void Publish(TMessage message);
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

public interface IDisposableEvent<TMessage> : IEventMarker, IEventPublisher<TMessage>, IEventSubscriber<TMessage>,
    IDisposable {
    void ClearSubscriptions();
}

public sealed class DisposableEvent<TMessage> : IDisposableEvent<TMessage> {
    readonly EventCore<TMessage> core;
    
    public DisposableEvent() : this(new EventCore<TMessage>()) { }
    public DisposableEvent(int expectedSubscriberCount) :
        this(new EventCore<TMessage>(expectedSubscriberCount)) { }

    DisposableEvent(EventCore<TMessage> core) {
        this.core = core;
    }
    
    public IDisposable Subscribe(IEventHandler<TMessage> handler) => core.Subscribe(handler);
    public void Publish(TMessage message) => core.Publish(message);
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
}