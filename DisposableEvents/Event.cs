namespace DisposableEvents;

public struct EmptyEvent;

public interface IEventPublisher<in TMessage> {
    /// <summary>
    /// Publishes a value to all subscribed observers.
    /// </summary>
    /// <param name="message">The value to publish.</param>
    void Publish(TMessage message);
}
public interface IEventSubscriber<TMessage> {
    /// <summary>
    /// Subscribes to the event with an observer.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <param name="filters">The filters to assign to the subscription</param>
    /// <returns>A disposable subscription that can be used to unsubscribe.</returns>
    IDisposable Subscribe(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters);
}

public interface IEvent : IDisposable { };
public interface IEvent<TMessage> : IEventPublisher<TMessage>, IEventSubscriber<TMessage>, IEvent { }

public sealed class Event<TMessage> : IEvent<TMessage> {
    readonly EventCore<TMessage> core;

    public Event(int expectedSubscriberCount = 2) : this(new EventCore<TMessage>(expectedSubscriberCount)) { }
    public Event(EventCore<TMessage> core) {
        this.core = core;
    }
    
    public IDisposable Subscribe(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) {
        if (filters.Length == 0)
            return core.Subscribe(observer);
        
        var filteredObserver = new FilteredEventObserver<TMessage>(observer, new CompositeEventFilter<TMessage>(filters));
        return core.Subscribe(filteredObserver);
    }
    
    public void Publish(TMessage message) => core.Publish(message);
    public void Dispose() => core.Dispose();
}
public sealed class Event : IEvent<EmptyEvent> {
    readonly EventCore<EmptyEvent> core;
    
    public Event(int expectedSubscriberCount = 2) : this(new EventCore<EmptyEvent>(expectedSubscriberCount)) { }
    public Event(EventCore<EmptyEvent> core) {
        this.core = core;
    }
    
    public IDisposable Subscribe(IObserver<EmptyEvent> observer, params IEventFilter<EmptyEvent>[] filters) {
        if (filters.Length == 0)
            return core.Subscribe(observer);
        
        var filteredObserver = new FilteredEventObserver<EmptyEvent>(observer, new CompositeEventFilter<EmptyEvent>(filters));
        return core.Subscribe(filteredObserver);
    }
    
    public void Publish(EmptyEvent message) => core.Publish(message);
    public void Dispose() => core.Dispose();
}