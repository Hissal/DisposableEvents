namespace DisposableEvents;

/// <summary>
/// Provides a base implementation for event subscribers, handling subscription logic with optional filtering.
/// Required in netstandard2.0 where default interface methods are not supported.
/// </summary>
public abstract class AbstractSubscriber<TMessage> : IEventSubscriber<TMessage> {
    public abstract IDisposable Subscribe(IEventHandler<TMessage> handler);

    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter) {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, filter);
        return Subscribe(filteredHandler);
    }
    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering) {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, filters, ordering);
        return Subscribe(filteredHandler);
    }
}

public abstract class EventSubscribeForwardImplementation<TMessage> : IEventSubscriber<TMessage> {
    protected IEventSubscriber<TMessage> InnerSubscriber { get; init; }

    public IDisposable Subscribe(IEventHandler<TMessage> handler) {
        return InnerSubscriber.Subscribe(handler);
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter) {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, filter);
        return InnerSubscriber.Subscribe(filteredHandler);
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering) {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, filters, ordering);
        return InnerSubscriber.Subscribe(filteredHandler);
    }
}