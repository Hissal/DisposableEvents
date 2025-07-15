using DisposableEvents.Factories;

namespace DisposableEvents;

public sealed class FilteredEvent<T> : IEvent<T> {
    readonly EventCore<T> core;
    readonly IEventObserverFactory observerFactory;
    readonly IEventFilter<T>[] defaultFilters;

    public FilteredEvent(params IEventFilter<T>[] defaultFilters) : this(2, defaultFilters) { }

    public FilteredEvent(int expectedSubscriberCount, params IEventFilter<T>[] defaultFilters)
        : this(new EventCore<T>(expectedSubscriberCount), defaultFilters) { }

    public FilteredEvent(int expectedSubscriberCount, IEventObserverFactory observerFactory, params IEventFilter<T>[] defaultFilters)
        : this(new EventCore<T>(expectedSubscriberCount), observerFactory, defaultFilters) { }

    public FilteredEvent(EventCore<T> core, params IEventFilter<T>[] defaultFilters) : this(core,
        EventObserverFactory.Default, defaultFilters) { }

    public FilteredEvent(EventCore<T> core, IEventObserverFactory observerFactory, params IEventFilter<T>[] defaultFilters) {
        this.core = core;
        this.observerFactory = observerFactory;
        this.defaultFilters = defaultFilters;
    }

    public IDisposable Subscribe(IObserver<T> observer, params IEventFilter<T>[] filters) {
        var resolvedFilters = filters.Length == 0
            ? defaultFilters
            : defaultFilters.Concat(filters).ToArray();

        return core.Subscribe(observerFactory.Create(observer, resolvedFilters));
    }

    public void Publish(T message) => core.Publish(message);
    public void Dispose() => core.Dispose();
}