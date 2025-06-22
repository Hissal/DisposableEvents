namespace DisposableEvents;

public struct EmptyEvent;

public interface ISubscriber<T> {
    /// <summary>
    /// Subscribes to the event with an observer.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <param name="filters">The filters to assign to the subscription</param>
    /// <returns>A disposable subscription that can be used to unsubscribe.</returns>
    IDisposable Subscribe(IObserver<T> observer, params IEventFilter<T>[] filters);
}
public interface IPublisher<in T> {
    /// <summary>
    /// Publishes a value to all subscribed observers.
    /// </summary>
    /// <param name="value">The value to publish.</param>
    void Publish(T value);
}
public interface IEvent<T> : ISubscriber<T>, IPublisher<T>, IDisposable { }
public interface IEvent : IEvent<EmptyEvent> {  }

public class Event<T> : IEvent<T> {
    readonly EventCore<T> core = new();

    public IDisposable Subscribe(IObserver<T> observer, params IEventFilter<T>[] filters) {
        if (filters is null || filters.Length == 0)
            return core.Subscribe(observer);
        
        var filteredObserver = new FilteredEventReceiver<T>(observer, new MultiEventFilter<T>(filters));
        return core.Subscribe(filteredObserver);
    }
    
    public void Publish(T value) => core.Publish(value);
    
    public void Dispose() => core.Dispose();
}
public class Event : IEvent {
    readonly EventCore<EmptyEvent> core = new();
    
    public IDisposable Subscribe(IObserver<EmptyEvent> observer, params IEventFilter<EmptyEvent>[] filters) {
        if (filters is null || filters.Length == 0)
            return core.Subscribe(observer);
        
        var filteredObserver = new FilteredEventReceiver<EmptyEvent>(observer, new MultiEventFilter<EmptyEvent>(filters));
        return core.Subscribe(filteredObserver);
    }
    
    public void Publish(EmptyEvent empty) => core.Publish(empty);
    public void Dispose() => core.Dispose();
}

internal class EventCore<T> : IDisposable {
    readonly List<Subscription> subscriptions = new();

    public IDisposable Subscribe(IObserver<T> observer) {
        var sub = new Subscription(observer, this);
        subscriptions.Add(sub);
        return sub;
    }

    public void Publish(T value) {
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
    
    public void Dispose() {
        foreach (var subscription in subscriptions) {
            subscription.Observer.OnCompleted();
        }
        subscriptions.Clear();
        GC.SuppressFinalize(this);
    }
    
    class Subscription : IDisposable {
        public IObserver<T> Observer { get; }
        public EventCore<T> Event { get; }

        public Subscription(IObserver<T> observer, EventCore<T> eventInstance) {
            Observer = observer ?? throw new ArgumentNullException(nameof(observer));
            Event = eventInstance ?? throw new ArgumentNullException(nameof(eventInstance));
        }

        public void Dispose() {
            Event.subscriptions.Remove(this);
        }
    }
}
