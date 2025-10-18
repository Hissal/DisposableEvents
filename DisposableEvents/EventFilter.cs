namespace DisposableEvents;

public interface IEventFilter {
    /// <summary>
    /// Gets the order in which this filter should be applied relative to other filters.
    /// </summary>
    int FilterOrder { get; }
}

public readonly record struct FilterResult(bool Passed) {
    public bool Blocked => !Passed;
    
    public static FilterResult Pass => new FilterResult(true);
    public static FilterResult Block => new FilterResult(false);
    
    public static implicit operator FilterResult(bool shouldPass) => new FilterResult(shouldPass);
    public static implicit operator bool(FilterResult result) => result.Passed;
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

/// <summary>
/// Defines a filter that uses a predicate to determine whether an event should be passed to handlers.
/// </summary>
/// <typeparam name="TMessage">The type of the event data.</typeparam>
public class PredicateEventFilter<TMessage> : IEventFilter<TMessage> {
    public int FilterOrder { get; }
    readonly System.Func<TMessage, bool> predicate;

    public PredicateEventFilter(System.Func<TMessage, bool> predicate) : this(0, predicate) { }
    public PredicateEventFilter(int filterOrder, System.Func<TMessage, bool> predicate) {
        FilterOrder = filterOrder;
        this.predicate = predicate;
    }

    public FilterResult Filter(ref TMessage value) => predicate(value);
}

public class StatefulPredicateEventFilter<TState, TMessage> : IEventFilter<TMessage> {
    public int FilterOrder { get; }
    readonly TState state;
    readonly Func<TState, TMessage, bool> predicate;

    public StatefulPredicateEventFilter(TState state, Func<TState, TMessage, bool> predicate) : this(0, state, predicate) { }
    public StatefulPredicateEventFilter(int filterOrder, TState state, Func<TState, TMessage, bool> predicate) {
        FilterOrder = filterOrder;
        this.state = state;
        this.predicate = predicate;
    }

    public FilterResult Filter(ref TMessage value) => predicate(state, value);
}

/// <summary>
/// Defines a filter that uses a predicate to determine whether an event should be passed to handlers.
/// </summary>
public class VoidPredicateEventFilter : IEventFilter<Void> {
    public int FilterOrder { get; }
    readonly Func<bool> predicate;

    public VoidPredicateEventFilter(Func<bool> predicate) : this(0, predicate) { }
    public VoidPredicateEventFilter(int filterOrder, Func<bool> predicate) {
        FilterOrder = filterOrder;
        this.predicate = predicate;
    }
    public FilterResult Filter(ref Void value) => predicate();
}

/// <summary>
/// Defines a filter that mutates the event data before passing it to handlers.
/// </summary>
/// <typeparam name="TMessage">The type of the event data.</typeparam>
public class ValueMutatorFilter<TMessage> : IEventFilter<TMessage> {
    public int FilterOrder { get; }
    readonly System.Func<TMessage, TMessage> mutator;

    public ValueMutatorFilter(System.Func<TMessage, TMessage> mutator) : this(0, mutator) { }
    public ValueMutatorFilter(int filterOrder, System.Func<TMessage, TMessage> mutator) {
        FilterOrder = filterOrder;
        this.mutator = mutator;
    }

    public FilterResult Filter(ref TMessage value) {
        value = mutator(value);
        return FilterResult.Pass;
    }
}