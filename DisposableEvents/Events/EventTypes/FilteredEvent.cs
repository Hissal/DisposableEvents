namespace DisposableEvents;

public sealed class FilteredEvent<TMessage> : IPipelineEvent<TMessage> {
    readonly LazyInnerEvent<TMessage> core;
    readonly IEventFilter<TMessage> filter;
    
    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;
    
    public FilteredEvent(IEnumerable<IEventFilter<TMessage>> filters, int initialSubscriberCapacity = -1) 
        : this(CompositeEventFilter<TMessage>.Create(filters), initialSubscriberCapacity) { }
    public FilteredEvent(IEnumerable<IEventFilter<TMessage>> filters, FilterOrdering ordering, int initialSubscriberCapacity = -1) 
        : this(CompositeEventFilter<TMessage>.Create(filters, ordering), initialSubscriberCapacity) { }
    
    public FilteredEvent(IEventFilter<TMessage>[] filters, int initialSubscriberCapacity = -1) 
        : this(CompositeEventFilter<TMessage>.Create(filters), initialSubscriberCapacity) { }
    public FilteredEvent(IEventFilter<TMessage>[] filters, FilterOrdering ordering, int initialSubscriberCapacity = -1) 
        : this(CompositeEventFilter<TMessage>.Create(filters, ordering), initialSubscriberCapacity) { }
    
    public FilteredEvent(params IEventFilter<TMessage>[] filters) 
        : this(CompositeEventFilter<TMessage>.Create(filters)) { }
    public FilteredEvent(FilterOrdering ordering, params IEventFilter<TMessage>[] filters) 
        : this(CompositeEventFilter<TMessage>.Create(filters, ordering)) { }
    
    public FilteredEvent(IEventFilter<TMessage> filter, int initialSubscriberCapacity = -1) {
        var capacity = initialSubscriberCapacity < 0 ? GlobalConfig.InitialSubscriberCapacity : initialSubscriberCapacity;
        this.core = new LazyInnerEvent<TMessage>(capacity);
        this.filter = filter;
    }
    
    public void Publish(TMessage message) {
        if (filter.Filter(ref message).Blocked)
            return;
        
        core.Publish(message);
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler) => core.Subscribe(handler);
    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage> handlerFilter) => core.Subscribe(handler, handlerFilter);
    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering) => core.Subscribe(handler, filters, ordering);

    public ReadOnlySpan<IEventHandler<TMessage>> GetHandlers() => core.GetHandlers();
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();

    IPipelineEvent<TMessage>? IPipelineEvent<TMessage>.Next => core.Next;
    void IPipelineEvent<TMessage>.SetNext(IPipelineEvent<TMessage> next) => core.SetNext(next);
}

public static class EventPipelineFilterExtensions {
    public static EventPipelineBuilder<TMessage> Filter<TMessage>(this EventPipelineBuilder<TMessage> builder, IEnumerable<IEventFilter<TMessage>> filters, FilterOrdering ordering = FilterOrdering.StableSort) {
        var preFilteredEvent = new FilteredEvent<TMessage>(filters, ordering);
        return builder.Next(preFilteredEvent);
    }
    
    public static EventPipelineBuilder<TMessage> Filter<TMessage>(this EventPipelineBuilder<TMessage> builder, IEventFilter<TMessage>[] filters, FilterOrdering ordering) {
        var preFilteredEvent = new FilteredEvent<TMessage>(filters, ordering);
        return builder.Next(preFilteredEvent);
    }
    
    public static EventPipelineBuilder<TMessage> Filter<TMessage>(this EventPipelineBuilder<TMessage> builder, params IEventFilter<TMessage>[] filters) {
        var preFilteredEvent = new FilteredEvent<TMessage>(filters);
        return builder.Next(preFilteredEvent);
    }
    
    public static EventPipelineBuilder<TMessage> Filter<TMessage>(this EventPipelineBuilder<TMessage> builder, FilterOrdering ordering, params IEventFilter<TMessage>[] filters) {
        var preFilteredEvent = new FilteredEvent<TMessage>(filters, ordering);
        return builder.Next(preFilteredEvent);
    }
    
    public static EventPipelineBuilder<TMessage> Filter<TMessage>(this EventPipelineBuilder<TMessage> builder, IEventFilter<TMessage> filter) {
        var preFilteredEvent = new FilteredEvent<TMessage>(filter);
        return builder.Next(preFilteredEvent);
    }
    
    public static EventPipelineBuilder<TMessage> Filter<TMessage>(this EventPipelineBuilder<TMessage> builder, System.Func<TMessage, bool> predicateFilter) {
        var preFilteredEvent = new FilteredEvent<TMessage>(new PredicateEventFilter<TMessage>(predicateFilter));
        return builder.Next(preFilteredEvent);
    }
    public static EventPipelineBuilder<TMessage> Filter<TState, TMessage>(this EventPipelineBuilder<TMessage> builder, TState state, Func<TState, TMessage, bool> predicateFilter) {
        var preFilteredEvent = new FilteredEvent<TMessage>(new PredicateEventFilter<TState, TMessage>(state, predicateFilter));
        return builder.Next(preFilteredEvent);
    }
}