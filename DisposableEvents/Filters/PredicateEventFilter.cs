namespace DisposableEvents;

/// <summary>
/// Defines a filter that uses a predicate to determine whether an event should be passed to handlers.
/// </summary>
/// <typeparam name="TMessage">The type of the event data.</typeparam>
public sealed class PredicateEventFilter<TMessage> : IEventFilter<TMessage> {
    public int FilterOrder { get; }
    readonly Func<TMessage, bool> predicate;

    public PredicateEventFilter(Func<TMessage, bool> predicate) : this(0, predicate) { }
    public PredicateEventFilter(int filterOrder, Func<TMessage, bool> predicate) {
        FilterOrder = filterOrder;
        this.predicate = predicate;
    }

    public FilterResult Filter(ref TMessage value) => predicate(value);
}

public sealed class PredicateEventFilter<TState, TMessage> : IEventFilter<TMessage> {
    public int FilterOrder { get; }
    readonly TState state;
    readonly Func<TState, TMessage, bool> predicate;

    public PredicateEventFilter(TState state, Func<TState, TMessage, bool> predicate) : this(state, 0, predicate) { }
    public PredicateEventFilter(TState state, int filterOrder, Func<TState, TMessage, bool> predicate) {
        FilterOrder = filterOrder;
        this.state = state;
        this.predicate = predicate;
    }

    public FilterResult Filter(ref TMessage value) => predicate(state, value);
}

/// <summary>
/// Defines a filter that uses a predicate to determine whether an event should be passed to handlers.
/// </summary>
public sealed class VoidPredicateEventFilter : IEventFilter<Void> {
    public int FilterOrder { get; }
    readonly Func<bool> predicate;

    public VoidPredicateEventFilter(Func<bool> predicate) : this(0, predicate) { }
    public VoidPredicateEventFilter(int filterOrder, Func<bool> predicate) {
        FilterOrder = filterOrder;
        this.predicate = predicate;
    }
    public FilterResult Filter(ref Void value) => predicate();
}

public sealed class VoidPredicateEventFilter<TState> : IEventFilter<Void> {
    public int FilterOrder { get; }
    readonly TState state;
    readonly Func<TState, bool> predicate;

    public VoidPredicateEventFilter(TState state, Func<TState, bool> predicate) : this(state, 0, predicate) { }
    public VoidPredicateEventFilter(TState state, int filterOrder, Func<TState, bool> predicate) {
        FilterOrder = filterOrder;
        this.state = state;
        this.predicate = predicate;
    }

    public FilterResult Filter(ref Void value) => predicate(state);
}