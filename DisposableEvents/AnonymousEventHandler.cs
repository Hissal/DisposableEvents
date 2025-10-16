namespace DisposableEvents;

public sealed class AnonymousEventHandler<TMessage> : IEventHandler<TMessage> {
    readonly Action<TMessage> onNext;

    public static implicit operator AnonymousEventHandler<TMessage>(Action<TMessage> onNext) => new AnonymousEventHandler<TMessage>(onNext);
    
    public AnonymousEventHandler(Action<TMessage> onNext) {
        this.onNext = onNext;
    }

    public void Handle(TMessage message) {
        onNext.Invoke(message);
    }

    public void OnUnsubscribe() { }
}

public sealed class FilteredEventHandler<TMessage> : IEventHandler<TMessage> {
    readonly IEventHandler<TMessage> handler;
    readonly IEventFilter<TMessage> filter;

    public FilteredEventHandler(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter) {
        this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
        this.filter = filter ?? throw new ArgumentNullException(nameof(filter));
    }

    public void Handle(TMessage message) {
        if (filter.Filter(ref message)) {
            handler.Handle(message);
        }
    }

    public void OnUnsubscribe() {
        handler.OnUnsubscribe();
    }
}

public sealed class ClosureEventObserver<TClosure, TMessage> : IObserver<TMessage> {
    readonly Action<TClosure, TMessage> onNext;
    readonly Action<TClosure, Exception>? onError;
    readonly Action<TClosure>? onCompleted;

    readonly TClosure closure;

    public ClosureEventObserver(TClosure closure, Action<TClosure, TMessage> onNext,
        Action<TClosure, Exception>? onError = null, Action<TClosure>? onCompleted = null) {
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