namespace DisposableEvents;

public sealed class FilteredEvent<T> : IEvent<T> {
    readonly EventCore<T> core;
    readonly IEventFilter<T>[] defaultFilters;

    public FilteredEvent(params IEventFilter<T>[] defaultFilters) : this(2, defaultFilters) { }
    public FilteredEvent(int expectedSubscriberCount = 2, params IEventFilter<T>[] defaultFilters) : this(new EventCore<T>(expectedSubscriberCount), defaultFilters) { }
    public FilteredEvent(EventCore<T> core, params IEventFilter<T>[] defaultFilters) {
        this.core = core;
        this.defaultFilters = defaultFilters;
    }
    
    public IDisposable Subscribe(IObserver<T> observer, params IEventFilter<T>[] filters) {
        var resolvedFilters = filters.Length == 0
            ? defaultFilters
            : defaultFilters.Concat(filters).ToArray();
        
        return core.Subscribe(new FilteredEventObserver<T>(observer, new CompositeEventFilter<T>(resolvedFilters)));
    }
    
    public void Publish(T message) => core.Publish(message);
    public void Dispose() => core.Dispose();
}