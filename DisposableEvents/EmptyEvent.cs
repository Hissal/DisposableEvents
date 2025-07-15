using DisposableEvents.Factories;

namespace DisposableEvents;

public struct EmptyEvent { }

public sealed class Event : IEvent<EmptyEvent> {
    readonly EventCore<EmptyEvent> core;
    readonly IEventObserverFactory observerFactory;
    
    public Event(int expectedSubscriberCount = 2, IEventObserverFactory? observerFactory = null) 
        : this(new EventCore<EmptyEvent>(expectedSubscriberCount), observerFactory) { }
    
    public Event(EventCore<EmptyEvent> core, IEventObserverFactory? observerFactory = null) {
        this.core = core;
        this.observerFactory = observerFactory ?? EventObserverFactory.Default;
    }
    
    public IDisposable Subscribe(IObserver<EmptyEvent> observer, params IEventFilter<EmptyEvent>[] filters) {
        return core.Subscribe(observerFactory.Create(observer, filters));
    }
    
    public void Publish(EmptyEvent message) => core.Publish(message);
    public void Dispose() => core.Dispose();
}

public sealed class EmptyEventObserver : IObserver<EmptyEvent> {
    readonly Action? onNext;
    readonly Action<Exception>? onError;
    readonly Action? onCompleted;

    public EmptyEventObserver(Action onNext, Action<Exception>? onError = null, Action? onCompleted = null) {
        this.onNext = onNext;
        this.onError = onError;
        this.onCompleted = onCompleted;
    }

    public void OnNext(EmptyEvent value) => onNext?.Invoke();
    public void OnError(Exception error) {
        if (onError == null)
            throw error;
        
        onError.Invoke(error);
    }

    public void OnCompleted() => onCompleted?.Invoke();
}

public sealed class EmptyClosureEventObserver<TClosure> : IObserver<EmptyEvent> {
    readonly Action<TClosure>? onNext;
    readonly Action<TClosure, Exception>? onError;
    readonly Action<TClosure>? onCompleted;

    readonly TClosure closure;
    
    public EmptyClosureEventObserver(TClosure closure, Action<TClosure> onNext, Action<TClosure, Exception>? onError = null, Action<TClosure>? onCompleted = null) {
        this.onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
        this.onError = onError;
        this.onCompleted = onCompleted;
        this.closure = closure;
    }

    public void OnNext(EmptyEvent value) => onNext?.Invoke(closure);
    public void OnError(Exception error) {
        if (onError == null)
            throw error;

        onError.Invoke(closure, error);
    }

    public void OnCompleted() => onCompleted?.Invoke(closure);
}

public class EmptyPredicateEventFilter : IEventFilter<EmptyEvent> {
    readonly Func<bool>? eventFilter;
    readonly Func<Exception, bool>? errorFilter;
    readonly Func<bool>? completedFilter;
    
    public int FilterOrder { get; }

    public EmptyPredicateEventFilter(Func<bool>? eventFilter = null, Func<Exception, bool>? errorFilter = null, Func<bool>? completedFilter = null) {
        FilterOrder = 0;
        
        this.eventFilter = eventFilter;
        this.errorFilter = errorFilter;
        this.completedFilter = completedFilter;
    }
    public EmptyPredicateEventFilter(int filterOrder, Func<bool>? eventFilter = null, Func<Exception, bool>? errorFilter = null, Func<bool>? completedFilter = null) {
        FilterOrder = filterOrder;
        
        this.eventFilter = eventFilter;
        this.errorFilter = errorFilter;
        this.completedFilter = completedFilter;
    }

    public bool FilterEvent(ref EmptyEvent value) => eventFilter?.Invoke() ?? true;
    public bool FilterOnError(Exception ex) => errorFilter?.Invoke(ex) ?? true;
    public bool FilterOnCompleted() => completedFilter?.Invoke() ?? true;
}