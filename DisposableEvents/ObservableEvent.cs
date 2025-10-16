namespace DisposableEvents;

public sealed class ObservableEvent<TMessage> : IDisposableEvent<TMessage>, IObservable<TMessage> {
    readonly IDisposableEvent<TMessage> wrappedEvent;

    public ObservableEvent(IDisposableEvent<TMessage> eventToWrap) {
        wrappedEvent = eventToWrap ?? throw new ArgumentNullException(nameof(eventToWrap));
    }
    
    public IDisposable Subscribe(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) => wrappedEvent.Subscribe(new ObserverEventHandler<TMessage>(observer), filters);
    public IDisposable Subscribe(IObserver<TMessage> observer) => wrappedEvent.Subscribe(new ObserverEventHandler<TMessage>(observer));

    public void Publish(TMessage message) => wrappedEvent.Publish(message);

    public void ClearSubscriptions() {
        throw new NotImplementedException();
    }

    public void Dispose() {
        wrappedEvent.Dispose();
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler, params IEventFilter<TMessage>[] filters) {
        return wrappedEvent.Subscribe(handler, filters);
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler) {
        return wrappedEvent.Subscribe(handler);
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter) {
        throw new NotImplementedException();
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering) {
        throw new NotImplementedException();
    }
}

public class ObserverEventHandler<TMessage> : IEventHandler<TMessage>, IObserver<TMessage> {
    readonly IObserver<TMessage> observer;

    public ObserverEventHandler(IObserver<TMessage> observer) {
        this.observer = observer ?? throw new ArgumentNullException(nameof(observer));
    }
    
    public void OnCompleted() => observer.OnCompleted();
    public void OnError(Exception error) => observer.OnError(error);
    public void OnNext(TMessage value) => observer.OnNext(value);
    public void Handle(TMessage message) {
        OnNext(message);
    }
    public void OnUnsubscribe() {
        OnCompleted();
    }
}

public static class ObservableEventExtensions {
    public static ObservableEvent<TMessage> AsObservable<TMessage>(this IDisposableEvent<TMessage> eventToWrap) => new ObservableEvent<TMessage>(eventToWrap);
}