
namespace DisposableEvents;

/// <summary>
/// Marker Interface for events to allow for non-generic storage and disposal.
/// </summary>
public interface IEventMarker;

public interface IEventPublisher<in TMessage> : IDisposable {
    public bool IsDisposed { get; }
    
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

public interface IDisposableEvent<TMessage> : IEventPublisher<TMessage>, IEventSubscriber<TMessage>, IEventMarker {
    void ClearSubscriptions();
}

public sealed class DisposableEvent<TMessage> : IDisposableEvent<TMessage> {
    readonly EventCore<TMessage> core;
    public bool IsDisposed => core.IsDisposed;
    
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

public sealed class DisposableEvent : IDisposableEvent<Void> {
    readonly EventCore<Void> core;
    public bool IsDisposed => core.IsDisposed;
    
    public DisposableEvent() : this(new EventCore<Void>()) { }
    public DisposableEvent(int expectedSubscriberCount) :
        this(new EventCore<Void>(expectedSubscriberCount)) { }

    DisposableEvent(EventCore<Void> core) {
        this.core = core;
    }
    
    public IDisposable Subscribe(IEventHandler<Void> handler) => core.Subscribe(handler);
    public void Publish(Void message) => core.Publish(message);
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
}