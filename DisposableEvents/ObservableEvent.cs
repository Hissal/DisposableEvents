namespace DisposableEvents;

public class ObservableEvent<T> : IEvent<T>, IObservable<T> {
    readonly IEvent<T> wrappedEvent;

    public ObservableEvent(IEvent<T> eventToWrap) {
        wrappedEvent = eventToWrap ?? throw new ArgumentNullException(nameof(eventToWrap));
    }
    
    public IDisposable Subscribe(IObserver<T> observer, params IEventFilter<T>[] filters) => wrappedEvent.Subscribe(observer, filters);
    public IDisposable Subscribe(IObserver<T> observer) => wrappedEvent.Subscribe(observer);

    public void Publish(T value) => wrappedEvent.Publish(value);

    public void Dispose() {
        wrappedEvent.Dispose();
        GC.SuppressFinalize(this);
    }
}

public static class ObservableEventExtensions {
    public static ObservableEvent<T> ToObservableEvent<T>(this IEvent<T> eventToWrap) => new(eventToWrap);
}