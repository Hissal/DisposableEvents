using DisposableEvents.Factories;

namespace DisposableEvents;

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
    readonly IEventObserverFactory observerFactory;

    public Event(int expectedSubscriberCount = 2, IEventObserverFactory? observerFactory = null) : this(new EventCore<TMessage>(expectedSubscriberCount), observerFactory) { }
    public Event(EventCore<TMessage> core, IEventObserverFactory? observerFactory = null) {
        this.core = core;
        this.observerFactory = observerFactory ?? EventObserverFactory.Default;
    }
    
    public IDisposable Subscribe(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) {
        return core.Subscribe(observerFactory.Create(observer, filters));
    }
    
    public void Publish(TMessage message) => core.Publish(message);
    public void Dispose() => core.Dispose();
}