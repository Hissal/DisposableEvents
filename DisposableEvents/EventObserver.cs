namespace DisposableEvents;

public sealed class EventObserver<T> : IObserver<T> {
    readonly Action<T> onNext;
    readonly Action<Exception>? onError;
    readonly Action? onCompleted;

    public EventObserver(Action<T> onNext, Action<Exception>? onError = null, Action? onCompleted = null) {
        this.onNext = onNext;
        this.onError = onError;
        this.onCompleted = onCompleted;
    }

    public void OnNext(T value) => onNext?.Invoke(value);
    public void OnError(Exception error) {
        if (onError == null)
            throw error;
        
        onError.Invoke(error);
    }
    public void OnCompleted() => onCompleted?.Invoke();
}

public sealed class EventObserver : IObserver<EmptyEvent> {
    readonly Action onNext;
    readonly Action<Exception>? onError;
    readonly Action? onCompleted;

    public EventObserver(Action onNext, Action<Exception>? onError = null, Action? onCompleted = null) {
        this.onNext = onNext;
        this.onError = onError;
        this.onCompleted = onCompleted;
    }

    public void OnNext(EmptyEvent value) => onNext?.Invoke();
    public void OnError(Exception error) {
        if (onError == null)
            throw error;
        
        onError.Invoke(error);
    }

    public void OnCompleted() => onCompleted?.Invoke();
}

public sealed class FilteredEventObserver<T> : IObserver<T> {
    readonly IObserver<T> observer;
    readonly IEventFilter<T> filter;

    public FilteredEventObserver(IObserver<T> observer, IEventFilter<T> filter) {
        this.observer = observer ?? throw new ArgumentNullException(nameof(observer));
        this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }
    
    public void OnNext(T value) {
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