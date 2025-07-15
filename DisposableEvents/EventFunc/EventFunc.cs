using DisposableEvents.Factories;

namespace DisposableEvents;

public enum FuncResultBehavior {
    ReturnLastSuccess,
    ReturnFirstSuccess,
    ReturnFirstSuccessAndStop,
    ReturnLast,
    ReturnFirst,
    ReturnFirstAndStop,
}

public enum FuncResultEnumerationBehavior {
    ReturnAll,
    SkipFailures,
}

public interface IEventFuncPublisher<in TMessage, TReturn> {
    /// <summary>
    /// Publishes a message to the event.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="behavior">The behavior of the function result.</param>
    /// <returns>The result of the function call.</returns>
    FuncResult<TReturn> Publish(TMessage message, FuncResultBehavior behavior = FuncResultBehavior.ReturnLastSuccess);
    
    /// <summary>
    /// Publishes a message to the event and returns an enumerator for the results.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="behavior">The enumeration behavior.</param>
    /// <returns>An enumerator for the results of the function calls.</returns>
    IEnumerable<FuncResult<TReturn>> PublishEnumerator(TMessage message, FuncResultEnumerationBehavior behavior = FuncResultEnumerationBehavior.ReturnAll);
}

public interface IEventFuncSubscriber<TMessage, TReturn> {
    /// <summary>
    /// Subscribes to the event with an observer.
    /// </summary>
    /// <param name="observer">The observer to subscribe.</param>
    /// <param name="filters">The filters to assign to the subscription.</param>
    /// <returns>A disposable subscription that can be used to unsubscribe.</returns>
    IDisposable Subscribe(IEventFuncObserver<TMessage, TReturn> observer, params IEventFilter<TMessage>[] filters);
}

public interface IEventFunc : IDisposable { }
public interface IEventFunc<TMessage, TReturn> : IEventFuncPublisher<TMessage, TReturn>, IEventFuncSubscriber<TMessage, TReturn>, IEventFunc { }

public interface IEventFuncObserver<in TMessage, TReturn> : IObserver<TMessage> {
    new FuncResult<TReturn> OnNext(TMessage value);
}

public class EventFunc<TMessage, TReturn> : IEventFunc<TMessage, TReturn> {
    readonly EventFuncCore<TMessage, TReturn> core;
    readonly IEventFuncObserverFactory observerFactory;

    public EventFunc(int expectedSubscriberCount = 2, IEventFuncObserverFactory? observerFactory = null) : this(new EventFuncCore<TMessage, TReturn>(expectedSubscriberCount), observerFactory) { }
    public EventFunc(EventFuncCore<TMessage, TReturn> core, IEventFuncObserverFactory? observerFactory = null) {
        this.core = core;
        this.observerFactory = observerFactory ?? EventFuncObserverFactory.Default;
    }
    
    public FuncResult<TReturn> Publish(TMessage message, FuncResultBehavior behavior = FuncResultBehavior.ReturnLastSuccess) => 
        core.Publish(message, behavior);
    
    public IEnumerable<FuncResult<TReturn>> PublishEnumerator(TMessage message, FuncResultEnumerationBehavior behavior = FuncResultEnumerationBehavior.ReturnAll) => 
        core.PublishEnumerator(message, behavior);
    
    public IDisposable Subscribe(IEventFuncObserver<TMessage, TReturn> observer, params IEventFilter<TMessage>[] filters) => 
        core.Subscribe(observerFactory.Create(observer, filters));

    public void Dispose() => core.Dispose();
}