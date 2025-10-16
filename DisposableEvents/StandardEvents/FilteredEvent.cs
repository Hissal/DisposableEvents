
namespace DisposableEvents;

// TODO: might want to use ArrayOrOne<T> for defaultFilters to avoid allocations in the common case of 1 default filter.
public sealed class FilteredEvent<TMessage> : IDisposableEvent<TMessage> {
    readonly EventCore<TMessage> core;
    readonly IEventFilter<TMessage>[] defaultFilters;
    
    public FilteredEvent(ReadOnlySpan<IEventFilter<TMessage>> defaultFilters) : this(new EventCore<TMessage>(), defaultFilters.ToArray()) { }
    public FilteredEvent(int expectedSubscriberCount, ReadOnlySpan<IEventFilter<TMessage>> defaultFilters)
        : this(new EventCore<TMessage>(expectedSubscriberCount), defaultFilters.ToArray()) { }
    
    public FilteredEvent(IEnumerable<IEventFilter<TMessage>> defaultFilters) : this(new EventCore<TMessage>(), defaultFilters.ToArray()) { }
    public FilteredEvent(int expectedSubscriberCount, IEnumerable<IEventFilter<TMessage>> defaultFilters)
        : this(new EventCore<TMessage>(expectedSubscriberCount), defaultFilters.ToArray()) { }
    
    public FilteredEvent(params IEventFilter<TMessage>[] defaultFilters)
        : this(new EventCore<TMessage>(), defaultFilters) { }
    public FilteredEvent(int expectedSubscriberCount, params IEventFilter<TMessage>[] defaultFilters)
        : this(new EventCore<TMessage>(expectedSubscriberCount), defaultFilters) { }

    FilteredEvent(EventCore<TMessage> core, params IEventFilter<TMessage>[] defaultFilters) {
        this.core = core;
        this.defaultFilters = defaultFilters;
    }

    public void Publish(TMessage message) {
        core.Publish(message);
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler) {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, defaultFilters);
        return core.Subscribe(filteredHandler);
    }

    IDisposable IEventSubscriber<TMessage>.Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter) {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, CombineFilters(filter));
        return core.Subscribe(filteredHandler);
    }
    
    IDisposable IEventSubscriber<TMessage>.Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering) {
        var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(handler, CombineFilters(filters), ordering);
        return core.Subscribe(filteredHandler);
    }
    
    IEventFilter<TMessage>[] CombineFilters(IEventFilter<TMessage>[] otherFilters) {
        var combined = new IEventFilter<TMessage>[defaultFilters.Length + otherFilters.Length];
        Array.Copy(defaultFilters, combined, defaultFilters.Length);
        Array.Copy(otherFilters, 0, combined, defaultFilters.Length, otherFilters.Length);
        return combined;
    }
    IEventFilter<TMessage>[] CombineFilters(IEventFilter<TMessage> otherFilter) {
        var combined = new IEventFilter<TMessage>[defaultFilters.Length + 1];
        Array.Copy(defaultFilters, combined, defaultFilters.Length);
        combined[defaultFilters.Length] = otherFilter;
        return combined;
    }
    
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
}