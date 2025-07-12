namespace DisposableEvents;

public struct EmptyEvent;

public interface IPublisher<in TMessage> {
    /// <summary>
    /// Publishes a value to all subscribed observers.
    /// </summary>
    /// <param name="value">The value to publish.</param>
    void Publish(TMessage value);
}
public interface ISubscriber<TMessage> {
    /// <summary>
    /// Subscribes to the event with an observer.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <param name="filters">The filters to assign to the subscription</param>
    /// <returns>A disposable subscription that can be used to unsubscribe.</returns>
    IDisposable Subscribe(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters);
}

public interface IEvent : IDisposable { };
public interface IEvent<TMessage> : IPublisher<TMessage>, ISubscriber<TMessage>, IEvent { }

public sealed class Event<TMessage> : IEvent<TMessage> {
    readonly EventCore<TMessage> core;

    public Event(EventCore<TMessage>? core = null) {
        this.core = core ?? new EventCore<TMessage>();
    }
    
    public IDisposable Subscribe(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) {
        if (filters.Length == 0)
            return core.Subscribe(observer);
        
        var filteredObserver = new FilteredEventObserver<TMessage>(observer, new MultiEventFilter<TMessage>(filters));
        return core.Subscribe(filteredObserver);
    }
    
    public void Publish(TMessage value) => core.Publish(value);
    public void Dispose() => core.Dispose();
}
public sealed class Event : IEvent<EmptyEvent> {
    readonly EventCore<EmptyEvent> core;
    
    public Event(EventCore<EmptyEvent>? core = null) {
        this.core = core ?? new EventCore<EmptyEvent>();
    }
    
    public IDisposable Subscribe(IObserver<EmptyEvent> observer, params IEventFilter<EmptyEvent>[] filters) {
        if (filters.Length == 0)
            return core.Subscribe(observer);
        
        var filteredObserver = new FilteredEventObserver<EmptyEvent>(observer, new MultiEventFilter<EmptyEvent>(filters));
        return core.Subscribe(filteredObserver);
    }
    
    public void Publish(EmptyEvent value) => core.Publish(value);
    public void Dispose() => core.Dispose();
}

public class EventCore<TMessage> : IDisposable {
    readonly List<Subscription> subscriptions = new();

    public IDisposable Subscribe(IObserver<TMessage> observer) {
        var sub = new Subscription(observer, this);
        subscriptions.Add(sub);
        return sub;
    }

    public void Publish(TMessage value) {
        if (subscriptions.Count == 0)
            return;

        foreach (var subscription in subscriptions) {
            try {
                subscription.Observer.OnNext(value);
            } catch (Exception ex) {
                subscription.Observer.OnError(ex);
            }
        }
    }
    
    ~EventCore() {
        Dispose();
    }
    
    public void Dispose() {
        foreach (var subscription in subscriptions) {
            subscription.Observer.OnCompleted();
        }
        subscriptions.Clear();
        GC.SuppressFinalize(this);
    }
    
    class Subscription : IDisposable {
        public IObserver<TMessage> Observer { get; }
        public EventCore<TMessage> Event { get; }

        public Subscription(IObserver<TMessage> observer, EventCore<TMessage> eventInstance) {
            Observer = observer ?? throw new ArgumentNullException(nameof(observer));
            Event = eventInstance ?? throw new ArgumentNullException(nameof(eventInstance));
        }

        public void Dispose() {
            Event.subscriptions.Remove(this);
        }
    }
}
