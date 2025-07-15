namespace DisposableEvents;

public sealed class EventObserver<TMessage> : IObserver<TMessage> {
    readonly Action<TMessage>? onNext;
    readonly Action<Exception>? onError;
    readonly Action? onCompleted;

    public EventObserver(Action<TMessage> onNext, Action<Exception>? onError = null, Action? onCompleted = null) {
        this.onNext = onNext;
        this.onError = onError;
        this.onCompleted = onCompleted;
    }

    public void OnNext(TMessage value) => onNext?.Invoke(value);
    public void OnError(Exception error) {
        if (onError == null)
            throw error;
        
        onError.Invoke(error);
    }
    public void OnCompleted() => onCompleted?.Invoke();
}

public sealed class FilteredEventObserver<TMessage> : IObserver<TMessage> {
    readonly IObserver<TMessage> observer;
    readonly IEventFilter<TMessage> filter;

    public FilteredEventObserver(IObserver<TMessage> observer, IEventFilter<TMessage> filter) {
        this.observer = observer ?? throw new ArgumentNullException(nameof(observer));
        this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }
    
    public void OnNext(TMessage value) {
        if (filter.FilterEvent(ref value)) {
            observer.OnNext(value);
        }
    }
    
    public void OnError(Exception error) {
        if (filter.FilterOnError(error)) {
            observer.OnError(error);
        }
    }
    
    public void OnCompleted() {
        if (filter.FilterOnCompleted()) {
            observer.OnCompleted();
        }
    }
}

public sealed class ClosureEventObserver<TClosure, TMessage> : IObserver<TMessage> {
    readonly Action<TClosure, TMessage> onNext;
    readonly Action<TClosure, Exception>? onError;
    readonly Action<TClosure>? onCompleted;
    
    readonly TClosure closure;

    public ClosureEventObserver(TClosure closure, Action<TClosure, TMessage> onNext, Action<TClosure, Exception>? onError = null, Action<TClosure>? onCompleted = null) {
        this.onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
        this.onError = onError;
        this.onCompleted = onCompleted;
        
        this.closure = closure;
    }

    public void OnNext(TMessage value) => onNext(closure, value);
    
    public void OnError(Exception error) {
        if (onError == null)
            throw error;
        
        onError.Invoke(closure, error);
    }
    
    public void OnCompleted() => onCompleted?.Invoke(closure);
}