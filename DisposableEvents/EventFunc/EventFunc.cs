using DisposableEvents.Factories;
using DisposableEvents.Internal;

namespace DisposableEvents;

public enum FuncResultBehavior {
    ReturnLastSuccess,
    ReturnFirstSuccess,
    ReturnFirstSuccessAndStop,
    ReturnLast,
    ReturnFirst,
    ReturnFirstAndStop,
}

[Flags]
public enum FuncResultEnumerationBehaviorFlags {
    None = 0,
    SkipFailures = 1 << 1,
    SkipErrors = 1 << 2,
    SkipSuccesses = 1 << 3,
    
    /// <summary>
    /// Skips failures and errors, returning only successes.
    /// </summary>
    Default = SkipFailures | SkipErrors,
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
    /// <param name="behaviorFlags">The enumeration behavior. <see cref="FuncResultEnumerationBehaviorFlags.Default"/> = SkipFailures | SkipErrors</param>
    /// <returns>An enumerator for the results of the function calls.</returns>
    IEnumerable<FuncResult<TReturn>> PublishEnumerator(TMessage message, FuncResultEnumerationBehaviorFlags behaviorFlags = FuncResultEnumerationBehaviorFlags.Default);
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
    void IObserver<TMessage>.OnNext(TMessage value) {
        OnNext(value);
    }
}

// public sealed class EventFunc<TReturn> : IEventFunc<EmptyEvent, TReturn> {
//     readonly EventFuncCore<EmptyEvent, TReturn> core;
//     readonly IEventFuncObserverFactory observerFactory;
//     
//     public EventFunc(int expectedSubscriberCount = 2, IEventFuncObserverFactory? observerFactory = null) : this(new EventFuncCore<EmptyEvent, TReturn>(expectedSubscriberCount), observerFactory) { }
//     public EventFunc(EventFuncCore<EmptyEvent, TReturn> core, IEventFuncObserverFactory? observerFactory = null) {
//         this.core = core;
//         this.observerFactory = observerFactory ?? EventFuncObserverFactory.Default;
//     }
//     
//     public FuncResult<TReturn> Publish(EmptyEvent message, FuncResultBehavior behavior = FuncResultBehavior.ReturnLastSuccess) => 
//         core.Publish(message, behavior);
//
//     public IEnumerable<FuncResult<TReturn>> PublishEnumerator(EmptyEvent message,
//         FuncResultEnumerationBehaviorFlags behaviorFlags = FuncResultEnumerationBehaviorFlags.Default) =>
//     core.PublishEnumerator(message, behaviorFlags);
//
//     public IDisposable Subscribe(IEventFuncObserver<EmptyEvent, TReturn> observer, params IEventFilter<EmptyEvent>[] filters) =>
//         core.Subscribe(observerFactory.Create(observer, filters));
//     
//     ~EventFunc() {
//         Dispose();
//     }
//     public void Dispose() {
//         core.Dispose();
//         GC.SuppressFinalize(this);
//     }
// }
public sealed class EventFunc<TMessage, TReturn> : IEventFunc<TMessage, TReturn> {
    readonly EventFuncCore<TMessage, TReturn> core;
    readonly IEventFuncObserverFactory observerFactory;

    public EventFunc(int expectedSubscriberCount = 2, IEventFuncObserverFactory? observerFactory = null) : this(new EventFuncCore<TMessage, TReturn>(expectedSubscriberCount), observerFactory) { }
    public EventFunc(EventFuncCore<TMessage, TReturn> core, IEventFuncObserverFactory? observerFactory = null) {
        this.core = core;
        this.observerFactory = observerFactory ?? EventFuncObserverFactory.Default;
    }
    
    public FuncResult<TReturn> Publish(TMessage message, FuncResultBehavior behavior = FuncResultBehavior.ReturnLastSuccess) => 
        core.Publish(message, behavior);
    
    public IEnumerable<FuncResult<TReturn>> PublishEnumerator(TMessage message, FuncResultEnumerationBehaviorFlags behaviorFlags = FuncResultEnumerationBehaviorFlags.Default) => 
        core.PublishEnumerator(message, behaviorFlags);
    
    public IDisposable Subscribe(IEventFuncObserver<TMessage, TReturn> observer, params IEventFilter<TMessage>[] filters) => 
        core.Subscribe(observerFactory.Create(observer, filters));

    ~EventFunc() {
        Dispose();
    }
    
    public void Dispose() {
        core.Dispose();
        GC.SuppressFinalize(this);
    }
}

public sealed class BufferedEventFunc<TMessage, TReturn> : IEventFunc<TMessage, TReturn> {
    readonly EventFuncCore<TMessage, TReturn> core;
    readonly IEventFuncObserverFactory observerFactory;
    
    TMessage? previousMessage;
    
    static readonly bool s_isValueType = typeof(TMessage).IsValueType;

    public BufferedEventFunc(int expectedSubscriberCount = 2, IEventFuncObserverFactory? observerFactory = null) : this(new EventFuncCore<TMessage, TReturn>(expectedSubscriberCount), observerFactory) { }
    public BufferedEventFunc(EventFuncCore<TMessage, TReturn> core, IEventFuncObserverFactory? observerFactory = null) {
        this.core = core;
        this.observerFactory = observerFactory ?? EventFuncObserverFactory.Default;
        previousMessage = default;
    }

    public FuncResult<TReturn> Publish(TMessage message, FuncResultBehavior behavior = FuncResultBehavior.ReturnLastSuccess) {
        previousMessage = message;
        return core.Publish(message, behavior);;
    }

    public IEnumerable<FuncResult<TReturn>> PublishEnumerator(TMessage message, FuncResultEnumerationBehaviorFlags behaviorFlags = FuncResultEnumerationBehaviorFlags.Default) {
        previousMessage = message;
        return core.PublishEnumerator(message, behaviorFlags);
    }

    public IDisposable Subscribe(IEventFuncObserver<TMessage, TReturn> observer, params IEventFilter<TMessage>[] filters) {
        ThrowHelper.ThrowIfNull(observer);
        return BufferedSubscribe(observerFactory.Create(observer, filters));
    }
    
    IDisposable BufferedSubscribe(IEventFuncObserver<TMessage, TReturn> observer) {
        if (!core.IsDisposed && (s_isValueType || previousMessage != null)) {
            try {
                observer.OnNext(previousMessage!);
            }
            catch (Exception e) {
                observer.OnError(e);
            }
        }
        
        return core.Subscribe(observer);
    }
    
    ~BufferedEventFunc() {
        Dispose();
    }

    public void Dispose() {
        core.Dispose();
        GC.SuppressFinalize(this);
    }
}