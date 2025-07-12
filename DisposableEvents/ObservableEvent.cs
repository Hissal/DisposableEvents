namespace DisposableEvents;

public sealed class ObservableEvent<TMessage> : IEvent<TMessage>, IObservable<TMessage> {
    readonly IEvent<TMessage> wrappedEvent;

    public ObservableEvent(IEvent<TMessage> eventToWrap) {
        wrappedEvent = eventToWrap ?? throw new ArgumentNullException(nameof(eventToWrap));
    }
    
    public IDisposable Subscribe(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) => wrappedEvent.Subscribe(observer, filters);
    public IDisposable Subscribe(IObserver<TMessage> observer) => wrappedEvent.Subscribe(observer);

    public void Publish(TMessage message) => wrappedEvent.Publish(message);

    public void Dispose() {
        wrappedEvent.Dispose();
    }
}

public static class ObservableEventExtensions {
    public static ObservableEvent<TMessage> AsObservable<TMessage>(this IEvent<TMessage> eventToWrap) => new ObservableEvent<TMessage>(eventToWrap);
}