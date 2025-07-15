namespace DisposableEvents.Factories;

public interface IEventObserverFactory {
    IObserver<T> Create<T>(IObserver<T> observer, IEventFilter<T>[] filters);
}

public class EventObserverFactory : IEventObserverFactory {
    public static IEventObserverFactory Default { get; } = new EventObserverFactory();
    
    public IObserver<T> Create<T>(IObserver<T> observer, IEventFilter<T>[] filters) {
        return filters.Length switch {
            0 => observer,
            1 => new FilteredEventObserver<T>(observer, filters[0]),
            _ => new FilteredEventObserver<T>(observer, new CompositeEventFilter<T>(filters))
        };
    }
}

public interface IEventFuncObserverFactory {
    IEventFuncObserver<TMessage, TReturn> Create<TMessage, TReturn>(IEventFuncObserver<TMessage, TReturn> observer, IEventFilter<TMessage>[] filters);
}
public class EventFuncObserverFactory : IEventFuncObserverFactory {
    public static IEventFuncObserverFactory Default { get; } = new EventFuncObserverFactory();
    
    public IEventFuncObserver<TMessage, TReturn> Create<TMessage, TReturn>(IEventFuncObserver<TMessage, TReturn> observer, IEventFilter<TMessage>[] filters) =>
        filters.Length switch {
            0 => observer,
            1 => new FilteredEventFuncObserver<TMessage, TReturn>(observer, filters[0]),
            _ => new FilteredEventFuncObserver<TMessage, TReturn>(observer, new CompositeEventFilter<TMessage>(filters))
        };
}