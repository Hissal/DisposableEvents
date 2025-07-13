namespace DisposableEvents;

/// <summary>
/// Defines a filter for event data, allowing control over which events, completion, or errors are passed to observers.
/// </summary>
/// <typeparam name="T">The type of the event data.</typeparam>
public interface IEventFilter<T> {
    /// <summary>
    /// Gets the order in which this filter should be applied relative to other filters.
    /// </summary>
    int FilterOrder { get; }
    
    /// <summary>
    /// Filters the event data before passing it to the observer.
    /// </summary>
    /// <param name="value">The value to filter.</param>
    /// <returns>True if the value should be passed to the observer, false otherwise.</returns>
    bool FilterEvent(ref T value);

    /// <summary>
    /// Determines whether the OnCompleted notification should be passed to the observer.
    /// </summary>
    /// <returns>True if the completion should be passed, false otherwise.</returns>
    bool FilterOnCompleted();

    /// <summary>
    /// Determines whether the OnError notification should be passed to the observer.
    /// </summary>
    /// <param name="ex">The exception to filter.</param>
    /// <returns>True if the error should be passed, false otherwise.</returns>
    bool FilterOnError(Exception ex);
}

/// <summary>
/// Defines a filter that uses a predicate to determine whether an event should be passed to observers.
/// </summary>
/// <typeparam name="T">The type of the event data.</typeparam>
public class PredicateEventFilter<T> : IEventFilter<T> {
    readonly Func<T, bool>? eventFilter;
    readonly Func<Exception, bool>? errorFilter;
    readonly Func<bool>? completedFilter;
    
    public int FilterOrder { get; }

    public PredicateEventFilter(Func<T, bool>? eventFilter = null, Func<Exception, bool>? errorFilter = null, Func<bool>? completedFilter = null) {
        FilterOrder = 0;
        
        this.eventFilter = eventFilter;
        this.errorFilter = errorFilter;
        this.completedFilter = completedFilter;
    }
    public PredicateEventFilter(int filterOrder, Func<T, bool>? eventFilter = null, Func<Exception, bool>? errorFilter = null, Func<bool>? completedFilter = null) {
        FilterOrder = filterOrder;
        
        this.eventFilter = eventFilter;
        this.errorFilter = errorFilter;
        this.completedFilter = completedFilter;
    }

    public bool FilterEvent(ref T value) => eventFilter?.Invoke(value) ?? true;
    public bool FilterOnError(Exception ex) => errorFilter?.Invoke(ex) ?? true;
    public bool FilterOnCompleted() => completedFilter?.Invoke() ?? true;
    
}

/// <summary>
/// Defines a composite event filter that applies multiple filters in order.
/// </summary>
/// <typeparam name="T">The type of the event data.</typeparam>
public class CompositeEventFilter<T> : IEventFilter<T> {
    readonly IEventFilter<T>[] orderedFilters;
    
    public int FilterOrder { get; }
    
    public CompositeEventFilter(params IEventFilter<T>[] filters) {
        FilterOrder = 0;
        orderedFilters = filters?.OrderBy(f => f.FilterOrder).ToArray()
                         ?? Array.Empty<IEventFilter<T>>();
    }
    public CompositeEventFilter(int filterOrder, params IEventFilter<T>[] filters) {
        FilterOrder = filterOrder;
        orderedFilters = filters?.OrderBy(f => f.FilterOrder).ToArray()
                         ?? Array.Empty<IEventFilter<T>>();
    }

    public bool FilterEvent(ref T value) {
        foreach (var filter in orderedFilters) {
            if (!filter.FilterEvent(ref value))
                return false;
        }
        return true;
    }
    
    public bool FilterOnCompleted() {
        foreach (var filter in orderedFilters) {
            if (!filter.FilterOnCompleted())
                return false;
        }
        return true;
    }
    
    public bool FilterOnError(Exception ex) {
        foreach (var filter in orderedFilters) {
            if (!filter.FilterOnError(ex))
                return false;
        }
        return true;
    }
}

public class MutatingEventFilter<T> : IEventFilter<T> {
    readonly Func<T, T>? eventFilter;
    
    public int FilterOrder { get; }

    public MutatingEventFilter(Func<T, T> filter) {
        FilterOrder = 0;
        eventFilter = filter;
    }
    public MutatingEventFilter(int filterOrder, Func<T, T> filter) {
        FilterOrder = filterOrder;
        eventFilter = filter;
    }

    public bool FilterEvent(ref T value) {
        if (eventFilter == null) 
            return true;
        
        value = eventFilter(value);
        return true;
    }

    public bool FilterOnCompleted() => true;
    public bool FilterOnError(Exception ex) => true;
}