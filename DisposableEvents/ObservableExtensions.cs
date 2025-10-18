using DisposableEvents.Internal;

namespace DisposableEvents;


public static class ObservableEventExtensions {
    public static IObservable<TMessage> AsObservable<TMessage>(this IEventSubscriber<TMessage> subscriber) {
        return new ObservableAdapter<TMessage>(subscriber);
    }
    
    public static IObservable<TMessage> AsObservable<TMessage>(this IEventSubscriber<TMessage> subscriber, IEventFilter<TMessage> filter) {
        return new ObservableAdapter<TMessage>(subscriber, filter);
    }
    
    public static IObservable<TMessage> AsObservable<TMessage>(this IEventSubscriber<TMessage> subscriber, params IEventFilter<TMessage>[] filters) {
        return filters.Length switch {
            0 => new ObservableAdapter<TMessage>(subscriber),
            1 => new ObservableAdapter<TMessage>(subscriber, filters[0]),
            _ => new ObservableAdapter<TMessage>(subscriber, filters)
        };
    }
}

internal sealed class ObservableAdapter<TMessage> : IObservable<TMessage> {
    readonly IEventSubscriber<TMessage> subscriber;
    readonly ArrayOrOne<IEventFilter<TMessage>>? filters;
    
    public ObservableAdapter(IEventSubscriber<TMessage> subscriber) {
        this.subscriber = subscriber;
        this.filters = null;
    }
    public ObservableAdapter(IEventSubscriber<TMessage> subscriber, IEventFilter<TMessage> filter) {
        this.subscriber = subscriber;
        this.filters = new ArrayOrOne<IEventFilter<TMessage>>(filter);
    }
    public ObservableAdapter(IEventSubscriber<TMessage> subscriber, params IEventFilter<TMessage>[] filters) {
        this.subscriber = subscriber;
        this.filters = new ArrayOrOne<IEventFilter<TMessage>>(filters);
    }
    
    public IDisposable Subscribe(IObserver<TMessage> observer) {
        var handler = new ObserverAdapter(observer);
        var subscription = filters switch {
            null => subscriber.Subscribe(handler),
            { IsArray: false } => subscriber.Subscribe(handler, filters[0]),
            { IsArray: true } => subscriber.Subscribe(handler, filters.AsArray(), FilterOrdering.StableSort)
        };
        handler.Subscription = subscription;
        return handler;
    }
    
    sealed class ObserverAdapter : IEventHandler<TMessage>, IDisposable {
        readonly IObserver<TMessage> observer;
        internal IDisposable? Subscription;
    
        public ObserverAdapter(IObserver<TMessage> observer) {
            this.observer = observer;
        }
    
        public void Handle(TMessage message) {
            try {
                observer.OnNext(message);
            }
            catch (Exception e) {
                observer.OnError(e);
            }
        }

        public void Dispose() {
            Subscription?.Dispose();
            observer.OnCompleted();
        }
    }
}