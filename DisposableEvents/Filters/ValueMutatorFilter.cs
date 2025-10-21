namespace DisposableEvents;

/// <summary>
/// Defines a filter that mutates the event data before passing it to handlers.
/// </summary>
/// <typeparam name="TMessage">The type of the event data.</typeparam>
public sealed class ValueMutatorFilter<TMessage> : IEventFilter<TMessage> {
    public int FilterOrder { get; }
    readonly Func<TMessage, TMessage> mutator;

    public ValueMutatorFilter(Func<TMessage, TMessage> mutator) : this(0, mutator) { }
    public ValueMutatorFilter(int filterOrder, Func<TMessage, TMessage> mutator) {
        FilterOrder = filterOrder;
        this.mutator = mutator;
    }

    public FilterResult Filter(ref TMessage value) {
        value = mutator(value);
        return FilterResult.Pass;
    }
}

public sealed class ValueMutatorFilter<TState, TMessage> : IEventFilter<TMessage> {
    public int FilterOrder { get; }
    readonly TState state;
    readonly Func<TState, TMessage, TMessage> mutator;

    public ValueMutatorFilter(TState state, Func<TState, TMessage, TMessage> mutator) : this(state, 0, mutator) { }
    public ValueMutatorFilter(TState state, int filterOrder,  Func<TState, TMessage, TMessage> mutator) {
        FilterOrder = filterOrder;
        this.state = state;
        this.mutator = mutator;
    }

    public FilterResult Filter(ref TMessage value) {
        value = mutator(state, value);
        return FilterResult.Pass;
    }
}