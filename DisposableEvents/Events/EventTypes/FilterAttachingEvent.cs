
using DisposableEvents.Internal;

namespace DisposableEvents;

// TODO: might want to use ArrayOrOne<T> for defaultFilters to avoid allocations in the common case of 1 default filter.
public sealed class FilterAttachingEvent<TMessage> : IPipelineEvent<TMessage> {
    readonly LazyInnerEvent<TMessage> core;
    readonly ArrayOrOne<IEventFilter<TMessage>> filters;
    
    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;

    public FilterAttachingEvent(ReadOnlySpan<IEventFilter<TMessage>> defaultFilters, int expectedSubscriberCount = -1)
        : this(defaultFilters.ToArray(), expectedSubscriberCount) { }
    
    public FilterAttachingEvent(IEnumerable<IEventFilter<TMessage>> defaultFilters, int expectedSubscriberCount = -1)
        : this(defaultFilters.ToArray(), expectedSubscriberCount) { }
    
    public FilterAttachingEvent(params IEventFilter<TMessage>[] filters) : this(filters, GlobalConfig.InitialSubscriberCapacity) { }

    public FilterAttachingEvent(IEventFilter<TMessage> filter, int initialSubscriberCapacity = -1) {
        core = new LazyInnerEvent<TMessage>(initialSubscriberCapacity);
        filters = new ArrayOrOne<IEventFilter<TMessage>>(filter);
    }

    public FilterAttachingEvent(IEventFilter<TMessage>[] filters, int initialSubscriberCapacity = -1) {
        core = new LazyInnerEvent<TMessage>(initialSubscriberCapacity);
        this.filters = filters;
    }

    public void Publish(TMessage message) {
        core.Publish(message);
    }


    public IDisposable Subscribe(IEventHandler<TMessage> handler) {
        return filters.IsArray 
            ? core.Subscribe(handler, filters.AsArray()) 
            : core.Subscribe(handler, filters.One);
    }

    IDisposable IEventSubscriber<TMessage>.Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter) {
        return core.Subscribe(handler, CombineFilters(filter));
    }
    
    IDisposable IEventSubscriber<TMessage>.Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering) {
        return core.Subscribe(handler, CombineFilters(filters), ordering);
    }
    
    IEventFilter<TMessage>[] CombineFilters(IEventFilter<TMessage>[] otherFilters) {
        if (filters.IsArray) {
            var combined = new IEventFilter<TMessage>[filters.Length + otherFilters.Length];
            Array.Copy(filters.AsArray(), combined, filters.Length);
            Array.Copy(otherFilters, 0, combined, filters.Length, otherFilters.Length);
            return combined;
        }
        
        var combinedSingle = new IEventFilter<TMessage>[1 + otherFilters.Length];
        combinedSingle[0] = filters.One;
        Array.Copy(otherFilters, 0, combinedSingle, 1, otherFilters.Length);
        return combinedSingle;
    }
    IEventFilter<TMessage>[] CombineFilters(IEventFilter<TMessage> otherFilter) {
        if (filters.IsArray) {
            var combined = new IEventFilter<TMessage>[filters.Length + 1];
            Array.Copy(filters.AsArray(), combined, filters.Length);
            combined[^1] = otherFilter;
            return combined;
        }
        
        var combinedSingle = new IEventFilter<TMessage>[2];
        combinedSingle[0] = filters.One;
        combinedSingle[1] = otherFilter;
        return combinedSingle;
    }
    
    public EventHandlerSnapshot<TMessage> SnapshotHandlers() => core.SnapshotHandlers();
    public void ClearHandlers() => core.ClearHandlers();
  
    public void Dispose() => core.Dispose();
    
    IPipelineEvent<TMessage>? IPipelineEvent<TMessage>.Next => core.Next;
    void IPipelineEvent<TMessage>.SetNext(IPipelineEvent<TMessage> next) => core.SetNext(next);
}

public static class EventPipelineFilterAttachingExtensions {
    public static EventPipelineBuilder<TMessage> AttachFilters<TMessage>(this EventPipelineBuilder<TMessage> builder, ReadOnlySpan<IEventFilter<TMessage>> filters) {
        var filterAttachingEvent = new FilterAttachingEvent<TMessage>(filters);
        return builder.Next(filterAttachingEvent);
    }
    
    public static EventPipelineBuilder<TMessage> AttachFilters<TMessage>(this EventPipelineBuilder<TMessage> builder, IEnumerable<IEventFilter<TMessage>> filters) {
        var filterAttachingEvent = new FilterAttachingEvent<TMessage>(filters);
        return builder.Next(filterAttachingEvent);
    }
    
    public static EventPipelineBuilder<TMessage> AttachFilters<TMessage>(this EventPipelineBuilder<TMessage> builder, params IEventFilter<TMessage>[] filters) {
        var filterAttachingEvent = new FilterAttachingEvent<TMessage>(filters);
        return builder.Next(filterAttachingEvent);
    }
    
    public static EventPipelineBuilder<TMessage> AttachFilters<TMessage>(this EventPipelineBuilder<TMessage> builder, IEventFilter<TMessage> filter) {
        var filterAttachingEvent = new FilterAttachingEvent<TMessage>(filter);
        return builder.Next(filterAttachingEvent);
    }
    
    public static EventPipelineBuilder<TMessage> AttachFilter<TMessage>(this EventPipelineBuilder<TMessage> builder, IEventFilter<TMessage> filter) {
        var filterAttachingEvent = new FilterAttachingEvent<TMessage>(filter);
        return builder.Next(filterAttachingEvent);
    }
    
    public static EventPipelineBuilder<TMessage> AttachFilter<TMessage>(this EventPipelineBuilder<TMessage> builder, System.Func<TMessage, bool> predicateFilter) {
        var filterAttachingEvent = new FilterAttachingEvent<TMessage>(new PredicateEventFilter<TMessage>(predicateFilter));
        return builder.Next(filterAttachingEvent);
    }
    
    public static EventPipelineBuilder<TMessage> AttachFilter<TState, TMessage>(this EventPipelineBuilder<TMessage> builder, TState state, Func<TState, TMessage, bool> predicateFilter) {
        var filterAttachingEvent = new FilterAttachingEvent<TMessage>(new PredicateEventFilter<TState, TMessage>(state, predicateFilter));
        return builder.Next(filterAttachingEvent);
    }
}