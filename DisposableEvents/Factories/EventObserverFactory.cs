namespace DisposableEvents.Factories;

public interface IEventObserverFactory {
    IObserver<T> Create<T>(IObserver<T> observer, IEventFilter<T>[] filters);
}

public class EventObserverFactory : IEventObserverFactory {
    public static IEventObserverFactory Default { get; } = new EventObserverFactory();
    
    public IObserver<T> Create<T>(IObserver<T> observer, IEventFilter<T>[] filters) =>
        filters.Length switch {
            0 => observer,
            1 => new FilteredEventObserver<T>(observer, filters[0]),
            _ => new FilteredEventObserver<T>(observer, new CompositeEventFilter<T>(filters))
        };
}