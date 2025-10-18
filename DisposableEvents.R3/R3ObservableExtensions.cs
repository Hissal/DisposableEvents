using DisposableEvents.Internal;
using R3;

namespace DisposableEvents.R3;

public static class R3ObservableExtensions {
    public static Observable<T> AsR3Observable<T>(this IEventSubscriber<T> source) {
        return new R3ObservableAdapter<T>(source);
    }
    public static Observable<T> AsR3Observable<T>(this IEventSubscriber<T> source, IEventFilter<T> filter) {
        return new R3ObservableAdapter<T>(source, filter);
    }

    public static Observable<T> AsR3Observable<T>(this IEventSubscriber<T> source, params IEventFilter<T>[] filters) {
        return filters.Length switch {
            0 => new R3ObservableAdapter<T>(source),
            1 => new R3ObservableAdapter<T>(source, filters[0]),
            _ => new R3ObservableAdapter<T>(source, filters)
        };
    }
}

internal sealed class R3ObservableAdapter<T> : Observable<T> {
    readonly IEventSubscriber<T> source;
    readonly ArrayOrOne<IEventFilter<T>>? filters;
    
    public R3ObservableAdapter(IEventSubscriber<T> source) {
        this.source = source;
        filters = null;
    }
    
    public R3ObservableAdapter(IEventSubscriber<T> source, IEventFilter<T> filter) {
        this.source = source;
        filters = new ArrayOrOne<IEventFilter<T>>(filter);
    }
    
    public R3ObservableAdapter(IEventSubscriber<T> source, IEventFilter<T>[] filters) {
        this.source = source;
        this.filters = new ArrayOrOne<IEventFilter<T>>(filters);
    }

    protected override IDisposable SubscribeCore(Observer<T> observer) {
        var adapter = new R3ObserverAdapter(observer);
        return filters switch {
            null => source.Subscribe(adapter),
            { IsArray: false } => source.Subscribe(adapter, filters.One),
            { IsArray: true } => source.Subscribe(adapter, filters.AsArray())
        };
    }
    
    sealed class R3ObserverAdapter : IEventHandler<T> {
        readonly Observer<T> observer;
        public R3ObserverAdapter(Observer<T> observer) => this.observer = observer;
        public void Handle(T message) => observer.OnNext(message);
    }
}