using System.Collections.Concurrent;
using DisposableEvents.Factories;

namespace DisposableEvents;

public readonly record struct Void;

public interface IEventHandler {
    void Handle();
}

public interface IFuncHandler<out TResult> {
    TResult Handle();
}

public interface IEventHandler<in TMessage> {
    void Handle(TMessage message);
}

public interface IFuncHandler<in TMessage, out TResult> {
    TResult Handle(TMessage message);
}

public interface IEventPublisher {
    /// <summary>
    /// Publishes a value to all subscribed observers.
    /// </summary>
    void Publish();
}

public interface IEventSubscriber {
    /// <summary>
    /// Subscribes to the event with an observer.
    /// </summary>
    /// <param name="handler">The handler to subscribe</param>
    /// <param name="filter">The filter to assign to the subscription</param>
    /// <returns>A disposable subscription that can be used to unsubscribe.</returns>
    IDisposable Subscribe(IEventHandler handler);
}

public interface IEventPublisher<in TMessage> {
    /// <summary>
    /// Publishes a value to all subscribed observers.
    /// </summary>
    /// <param name="message">The value to publish.</param>
    void Publish(TMessage message);
}

public interface IEventSubscriber<TMessage> {
    IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter);
    /// <summary>
    /// Subscribes to the event with an observer.
    /// </summary>
    /// <param name="handler">The handler to subscribe</param>
    /// <returns>A disposable subscription that can be used to unsubscribe.</returns>
    IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering);
}

/// <summary>
/// Marker Interface for events to allow for non-generic storage and disposal.
/// </summary>
public interface IEventMarker;

public interface IDisposableEvent : IEventMarker, IEventPublisher, IEventSubscriber, IDisposable {
    void ClearSubscriptions();
}

/// <summary>
/// An event that allows publishing messages of type TMessage to subscribed handlers.
/// </summary>
/// <typeparam name="TMessage">The message type</typeparam>
public interface IDisposableEvent<TMessage> : IEventMarker, IEventPublisher<TMessage>, IEventSubscriber<TMessage>,
    IDisposable {
    void ClearSubscriptions();
}

public sealed class DisposableEvent : IDisposableEvent {
    readonly EventCore core;

    public DisposableEvent(int? expectedSubscriberCount = null) :
        this(new EventCore(expectedSubscriberCount)) { }

    DisposableEvent(EventCore core) {
        this.core = core;
    }

    public IDisposable Subscribe(IEventHandler handler) => core.Subscribe(handler);

    public void Publish() => core.Publish();
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
}

public sealed class DisposableEvent<TMessage> : IDisposableEvent<TMessage> {
    readonly EventCore<TMessage> core;
    
    public DisposableEvent(int? expectedSubscriberCount = null) :
        this(new EventCore<TMessage>(expectedSubscriberCount)) { }

    DisposableEvent(EventCore<TMessage> core) {
        this.core = core;
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter) {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, filter);
        return core.Subscribe(filteredHandler);
    }
    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering) {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, filters, ordering);
        return core.Subscribe(filteredHandler);
    }
    
    public void Publish(TMessage message) => core.Publish(message);
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
}