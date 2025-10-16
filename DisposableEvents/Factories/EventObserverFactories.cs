namespace DisposableEvents.Factories;

public interface IFilteredEventHandlerFactory {
    IEventHandler<T> CreateFilteredHandler<T>(IEventHandler<T> handler, IEventFilter<T> filter);
    
    IEventHandler CreateFilteredHandler(IEventHandler handler, IEmptyEventFilter[] filters, FilterOrdering ordering = FilterOrdering.StableSort);
    IEventHandler<T> CreateFilteredHandler<T>(IEventHandler<T> handler, IEventFilter<T>[] filters, FilterOrdering ordering = FilterOrdering.StableSort);
}

public class FilteredEventHandlerFactory : IFilteredEventHandlerFactory {
    public static IFilteredEventHandlerFactory Default { get; } = new FilteredEventHandlerFactory();

    public IEventHandler<T> CreateFilteredHandler<T>(IEventHandler<T> handler, IEventFilter<T> filter) {
        return new FilteredEventHandler<T>(handler, filter);
    }
    
    public IEventHandler CreateFilteredHandler(IEventHandler handler, IEmptyEventFilter[] filters, FilterOrdering ordering = FilterOrdering.StableSort) {
        // return filters.Length switch {
        //     0 => handler,
        //     1 => new FilteredEventHandler(handler, filters[0]),
        //     _ => new FilteredEventHandler(handler, CompositeEventFilter.Create(filters, ordering))
        // };
        return null;
    }

    public IEventHandler<T> CreateFilteredHandler<T>(IEventHandler<T> handler, IEventFilter<T>[] filters, FilterOrdering ordering = FilterOrdering.StableSort) {
        return filters.Length switch {
            0 => handler,
            1 => new FilteredEventHandler<T>(handler, filters[0]),
            _ => new FilteredEventHandler<T>(handler, CompositeEventFilter<T>.Create(filters, ordering))
        };
    }
}

public interface IEventFuncObserverFactory {
    IEventFuncObserver<TMessage, TReturn> Create<TMessage, TReturn>(IEventFuncObserver<TMessage, TReturn> observer,
        IEventFilter<TMessage>[] filters);
}

public class EventFuncObserverFactory : IEventFuncObserverFactory {
    public static IEventFuncObserverFactory Default { get; } = new EventFuncObserverFactory();

    public IEventFuncObserver<TMessage, TReturn> Create<TMessage, TReturn>(
        IEventFuncObserver<TMessage, TReturn> observer, IEventFilter<TMessage>[] filters) =>
        filters.Length switch {
            0 => observer,
            1 => new FilteredEventFuncObserver<TMessage, TReturn>(observer, filters[0]),
            _ => new FilteredEventFuncObserver<TMessage, TReturn>(observer, CompositeEventFilter<TMessage>.Create(filters))
        };
}