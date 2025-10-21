namespace DisposableEvents;

public interface IEventFilter {
    /// <summary>
    /// Gets the order in which this filter should be applied relative to other filters.
    /// </summary>
    int FilterOrder { get; }
}

/// <summary>
/// Defines a filter for event data, allowing control over which events, completion, or errors are passed to observers.
/// </summary>
/// <typeparam name="TMessage">The type of the event data.</typeparam>
public interface IEventFilter<TMessage> : IEventFilter { 
    /// <summary>
    /// Filters the event data before passing it to the observer.
    /// </summary>
    /// <param name="value">The value to filter.</param>
    /// <returns>True if the value should be passed to the observer, false otherwise.</returns>
    FilterResult Filter(ref TMessage value);
}