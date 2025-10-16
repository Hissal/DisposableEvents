namespace DisposableEvents;

public sealed class PreFilteredEvent<TMessage> : IDisposableEvent<TMessage> {
    readonly EventCore<TMessage> core;
    readonly IEventFilter<TMessage> defaultFilter;
    
    public PreFilteredEvent(ReadOnlySpan<IEventFilter<TMessage>> filters) : this(new EventCore<TMessage>(), CompositeEventFilter<TMessage>.Create(filters, FilterOrdering.StableSort)) { }
    public PreFilteredEvent(int initialSubscriberCapacity, ReadOnlySpan<IEventFilter<TMessage>> filters)
        : this(new EventCore<TMessage>(initialSubscriberCapacity), CompositeEventFilter<TMessage>.Create(filters, FilterOrdering.StableSort)) { }
    public PreFilteredEvent(int initialSubscriberCapacity, FilterOrdering ordering, ReadOnlySpan<IEventFilter<TMessage>> filters) 
        : this(new EventCore<TMessage>(initialSubscriberCapacity), CompositeEventFilter<TMessage>.Create(filters, ordering)) { }
    
    public PreFilteredEvent(IEnumerable<IEventFilter<TMessage>> filters) : this(new EventCore<TMessage>(), CompositeEventFilter<TMessage>.Create(filters, FilterOrdering.StableSort)) { }
    public PreFilteredEvent(int initialSubscriberCapacity, IEnumerable<IEventFilter<TMessage>> filters)
        : this(new EventCore<TMessage>(initialSubscriberCapacity), CompositeEventFilter<TMessage>.Create(filters, FilterOrdering.StableSort)) { }
    public PreFilteredEvent(int initialSubscriberCapacity, FilterOrdering ordering, IEnumerable<IEventFilter<TMessage>> filters) 
        : this(new EventCore<TMessage>(initialSubscriberCapacity), CompositeEventFilter<TMessage>.Create(filters, ordering)) { }
    
    public PreFilteredEvent(params IEventFilter<TMessage>[] filters) 
        : this(new EventCore<TMessage>(), CompositeEventFilter<TMessage>.Create(filters, FilterOrdering.StableSort)) { }
    public PreFilteredEvent(int initialSubscriberCapacity, params IEventFilter<TMessage>[] filters) 
        : this(new EventCore<TMessage>(initialSubscriberCapacity), CompositeEventFilter<TMessage>.Create(filters, FilterOrdering.StableSort)) { }
    public PreFilteredEvent(int initialSubscriberCapacity, FilterOrdering ordering, params IEventFilter<TMessage>[] filters) 
        : this(new EventCore<TMessage>(initialSubscriberCapacity), CompositeEventFilter<TMessage>.Create(filters, ordering)) { }

    public PreFilteredEvent(IEventFilter<TMessage> defaultFilter) : this(new EventCore<TMessage>(), defaultFilter) { }
    public PreFilteredEvent(int initialSubscriberCapacity, IEventFilter<TMessage> defaultFilter)
        : this(new EventCore<TMessage>(initialSubscriberCapacity), defaultFilter) { }

    PreFilteredEvent(EventCore<TMessage> core, IEventFilter<TMessage> defaultFilter) {
        this.core = core;
        this.defaultFilter = defaultFilter;
    }

    public void Publish(TMessage message) {
        if (defaultFilter.Filter(ref message).Blocked)
            return;
        
        core.Publish(message);
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler) => core.Subscribe(handler);
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
}