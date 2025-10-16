using DisposableEvents.Internal;

namespace DisposableEvents;


public static class ObservableEventExtensions {
    public static IObservable<TMessage> AsObservable<TMessage>(this IEventSubscriber<TMessage> subscriber, params IEventFilter<TMessage>[] filters) {
        return filters.Length switch {
            0 => new ObservableSubscriber<TMessage>(subscriber),
            1 => new ObservableSubscriber<TMessage>(subscriber, filters[0]),
            _ => new ObservableSubscriber<TMessage>(subscriber, filters)
        };
    }
}

public sealed class ObservableSubscriber<TMessage> : IObservable<TMessage> {
    readonly IEventSubscriber<TMessage> subscriber;
    readonly ArrayOrOne<IEventFilter<TMessage>>? filters;
    
    public ObservableSubscriber(IEventSubscriber<TMessage> subscriber) {
        this.subscriber = subscriber;
        this.filters = null;
    }
    public ObservableSubscriber(IEventSubscriber<TMessage> subscriber, IEventFilter<TMessage> filter) {
        this.subscriber = subscriber;
        this.filters = new ArrayOrOne<IEventFilter<TMessage>>(filter);
    }
    public ObservableSubscriber(IEventSubscriber<TMessage> subscriber, params IEventFilter<TMessage>[] filters) {
        this.subscriber = subscriber;
        this.filters = new ArrayOrOne<IEventFilter<TMessage>>(filters);
    }
    
    public IDisposable Subscribe(IObserver<TMessage> observer) {
        var handler = new ObserverEventHandler<TMessage>(observer);
        return filters switch {
            null => subscriber.Subscribe(handler),
            { Length: 1 } => subscriber.Subscribe(handler, filters[0]),
            _ => subscriber.Subscribe(handler, filters!.AsArray(), FilterOrdering.StableSort)
        };
    }
}

public sealed class ObserverEventHandler<TMessage> : IEventHandler<TMessage>, IObserver<TMessage> {
    readonly IObserver<TMessage> observer;

    public ObserverEventHandler(IObserver<TMessage> observer) {
        this.observer = observer;
    }
    
    public void OnCompleted() => observer.OnCompleted();
    public void OnError(Exception error) => observer.OnError(error);
    public void OnNext(TMessage value) => observer.OnNext(value);
    
    public void Handle(TMessage message) {
        try {
            OnNext(message);
        }
        catch (Exception e) {
            OnError(e);
        }
    }
}