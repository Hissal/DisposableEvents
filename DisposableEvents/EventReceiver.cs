namespace DisposableEvents;

public class EventReceiver<T> : IObserver<T> {
    readonly Action<T> onNext;
    readonly Action<Exception>? onError;
    readonly Action? onCompleted;

    public EventReceiver(Action<T> onNext, Action<Exception>? onError = null, Action? onCompleted = null) {
        this.onNext = onNext;
        this.onError = onError;
        this.onCompleted = onCompleted;
    }

    public void OnNext(T value) => onNext?.Invoke(value);
    public void OnError(Exception error) => onError?.Invoke(error);
    public void OnCompleted() => onCompleted?.Invoke();
}

public class EventReceiver : IObserver<EmptyEvent> {
    readonly Action onNext;
    readonly Action<Exception>? onError;
    readonly Action? onCompleted;

    public EventReceiver(Action onNext, Action<Exception>? onError = null, Action? onCompleted = null) {
        this.onNext = onNext;
        this.onError = onError;
        this.onCompleted = onCompleted;
    }

    public void OnNext(EmptyEvent value) => onNext?.Invoke();
    public void OnError(Exception error) => onError?.Invoke(error);
    public void OnCompleted() => onCompleted?.Invoke();
}

public class FilteredEventReceiver<T> : IObserver<T> {
    readonly IObserver<T> handler;
    readonly IEventFilter<T> filter;

    public FilteredEventReceiver(IObserver<T> handler, IEventFilter<T> filter) {
        this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
        this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }
    
    public void OnNext(T value) {
        if (filter.FilterEvent(ref value)) {
            handler.OnNext(value);
        }
    }
    
    public void OnError(Exception error) {
        if (filter.FilterOnError(error)) {
            handler.OnError(error);
        }
    }
    
    public void OnCompleted() {
        if (filter.FilterOnCompleted()) {
            handler.OnCompleted();
        }
    }
}