namespace DisposableEvents;

public sealed class FilteredEvent<T> : IEvent<T> {
    readonly EventCore<T> core;
    readonly IEventFilter<T>[] defaultFilters;

    public FilteredEvent(params IEventFilter<T>[] defaultFilters) : this(null, defaultFilters) { }
    public FilteredEvent(EventCore<T>? core, params IEventFilter<T>[] defaultFilters) {
        this.core = core ?? new EventCore<T>();
        this.defaultFilters = defaultFilters;
    }
    
    public IDisposable Subscribe(IObserver<T> observer, params IEventFilter<T>[] filters) {
        var resolvedFilters = filters.Length == 0
            ? defaultFilters
            : defaultFilters.Concat(filters).ToArray();
        
        return core.Subscribe(new FilteredEventObserver<T>(observer, new MultiEventFilter<T>(resolvedFilters)));
    }
    
    public void Publish(T value) => core.Publish(value);
    public void Dispose() => core.Dispose();
}