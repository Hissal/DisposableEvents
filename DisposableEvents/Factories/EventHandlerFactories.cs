namespace DisposableEvents.Factories;

public interface IFilteredEventHandlerFactory {
    IEventHandler<T> CreateFilteredHandler<T>(IEventHandler<T> handler, IEventFilter<T> filter);
    IEventHandler<T> CreateFilteredHandler<T>(IEventHandler<T> handler, IEventFilter<T>[] filters, FilterOrdering ordering = FilterOrdering.StableSort);
}

public class FilteredEventHandlerFactory : IFilteredEventHandlerFactory {
    public static IFilteredEventHandlerFactory Default { get; } = new FilteredEventHandlerFactory();

    public IEventHandler<T> CreateFilteredHandler<T>(IEventHandler<T> handler, IEventFilter<T> filter) {
        return new FilteredEventHandler<T>(handler, filter);
    }
    
    public IEventHandler<T> CreateFilteredHandler<T>(IEventHandler<T> handler, IEventFilter<T>[] filters, FilterOrdering ordering = FilterOrdering.StableSort) {
        return filters.Length switch {
            0 => handler,
            1 => new FilteredEventHandler<T>(handler, filters[0]),
            _ => new FilteredEventHandler<T>(handler, CompositeEventFilter<T>.Create(filters, ordering))
        };
    }
}

public interface IFilteredFuncHandlerFactory {
    IFuncHandler<TMessage, TReturn> CreateFilteredHandler<TMessage, TReturn>(IFuncHandler<TMessage, TReturn> handler, IEventFilter<TMessage> filter);
    IFuncHandler<TMessage, TReturn> CreateFilteredHandler<TMessage, TReturn>(IFuncHandler<TMessage, TReturn> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering = FilterOrdering.StableSort);
}

public class FilteredFuncHandlerFactory : IFilteredFuncHandlerFactory {
    public static IFilteredFuncHandlerFactory Default { get; } = new FilteredFuncHandlerFactory();
    
    public IFuncHandler<TMessage, TReturn> CreateFilteredHandler<TMessage, TReturn>(IFuncHandler<TMessage, TReturn> handler, IEventFilter<TMessage> filter) {
        return new FilteredFuncHandler<TMessage, TReturn>(handler, filter);
    }

    public IFuncHandler<TMessage, TReturn> CreateFilteredHandler<TMessage, TReturn>(IFuncHandler<TMessage, TReturn> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering = FilterOrdering.StableSort) {
        return filters.Length switch {
            0 => handler,
            1 => new FilteredFuncHandler<TMessage, TReturn>(handler, filters[0]),
            _ => new FilteredFuncHandler<TMessage, TReturn>(handler, CompositeEventFilter<TMessage>.Create(filters, ordering))
        };
    }
}