namespace DisposableEvents;

public sealed class EventFuncObserver<TMessage, TReturn> : IEventFuncObserver<TMessage, TReturn> {
    readonly Func<TMessage, FuncResult<TReturn>> onNext;
    readonly Action<Exception>? onError;
    readonly Action? onCompleted;

    public EventFuncObserver(Func<TMessage, FuncResult<TReturn>> onNext, Action<Exception>? onError = null, Action? onCompleted = null) {
        this.onNext = onNext;
        this.onError = onError;
        this.onCompleted = onCompleted;
    }

    public FuncResult<TReturn> OnNext(TMessage value) => onNext.Invoke(value);
    void IObserver<TMessage>.OnNext(TMessage value) => OnNext(value);
    
    public void OnError(Exception error) {
        if (onError == null)
            throw error;
        
        onError.Invoke(error);
    }

    public void OnCompleted() => onCompleted?.Invoke();
}

public sealed class FilteredEventFuncObserver<TMessage, TReturn> : IEventFuncObserver<TMessage, TReturn> {
    readonly IEventFuncObserver<TMessage, TReturn> observer;
    readonly IEventFilter<TMessage> filter;

    public FilteredEventFuncObserver(IEventFuncObserver<TMessage, TReturn> observer, IEventFilter<TMessage> filter) {
        this.observer = observer ?? throw new ArgumentNullException(nameof(observer));
        this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }
    
    public FuncResult<TReturn> OnNext(TMessage value) {
        return filter.FilterEvent(ref value) 
            ? observer.OnNext(value) 
            : FuncResult<TReturn>.Failure();
    }
    void IObserver<TMessage>.OnNext(TMessage value) {
        OnNext(value);
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

public sealed class ClosureEventFuncObserver<TClosure, TMessage, TReturn> : IEventFuncObserver<TMessage, TReturn> {
    readonly Func<TClosure, TMessage, FuncResult<TReturn>> onNext;
    readonly Action<TClosure, Exception>? onError;
    readonly Action<TClosure>? onCompleted;
    
    readonly TClosure closure;

    public ClosureEventFuncObserver(TClosure closure, Func<TClosure, TMessage, FuncResult<TReturn>> onNext, Action<TClosure, Exception>? onError = null, Action<TClosure>? onCompleted = null) {
        this.onNext = onNext;
        this.onError = onError;
        this.onCompleted = onCompleted;
        
        this.closure = closure;
    }

    public FuncResult<TReturn> OnNext(TMessage value) => onNext.Invoke(closure, value);
    void IObserver<TMessage>.OnNext(TMessage value) => OnNext(value);
    
    public void OnError(Exception error) {
        if (onError == null)
            throw error;
        
        onError.Invoke(closure, error);
    }
    
    public void OnCompleted() => onCompleted?.Invoke(closure);
}