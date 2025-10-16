namespace DisposableEvents;

/// <summary>
/// Provides extension methods for subscribing to and publishing events with various observer and filter configurations.
/// </summary>
public static class SubscriberExtensions {
    public static IDisposable Subscribe<TMessage>(this IEventSubscriber<TMessage> subscriber,
        IEventHandler<TMessage> handler, params IEventFilter<TMessage>[] filters) {
        return subscriber.Subscribe(handler, filters, FilterOrdering.StableSort);
    }

    public static IDisposable Subscribe<TMessage>(this IEventSubscriber<TMessage> subscriber, Action<TMessage> handler,
        params IEventFilter<TMessage>[] filters) 
    {
        return subscriber.Subscribe(new AnonymousEventHandler<TMessage>(handler), filters);
    }

    // public static IDisposable SubscribePooled<TMessage>(this IEventSubscriber<TMessage> subscriber,
    //     IEventHandler<TMessage> handler, IEventFilter<TMessage> filter1, IEventFilter<TMessage> filter2, FilterOrdering ordering = FilterOrdering.StableSort)
    // {
    //     var filters = ArrayPool<IEventFilter<TMessage>>.Shared.Rent(2);
    //     filters[0] = filter1;
    //     filters[1] = filter2;
    //     
    //     try {
    //         var compositeFilter = CompositeEventFilter<TMessage>.Create(filters.AsSpan(0,2), ordering);
    //         var filteredHandler = new FilteredEventHandler<TMessage>(handler, compositeFilter);
    //         return subscriber.Subscribe(filteredHandler);
    //     }
    //     finally {
    //         ArrayPool<IEventFilter<TMessage>>.Shared.Return(filters);
    //     }
    // }
    //
    // public static IDisposable Subscribe<TMessage>(this IEventSubscriber<TMessage> subscriber,
    //     IEventHandler<TMessage> handler, IEventFilter<TMessage> filter1, IEventFilter<TMessage> filter2, FilterOrdering ordering = FilterOrdering.StableSort)
    // {
    //     var filters = new[] { filter1, filter2 };
    //     var compositeFilter = CompositeEventFilter<TMessage>.Create(filters, ordering);
    //     var filteredHandler = new FilteredEventHandler<TMessage>(handler, compositeFilter);
    //     return subscriber.Subscribe(filteredHandler);
    // }
    //
    // public static IDisposable Subscribe<TMessage>(this IEventSubscriber<TMessage> subscriber,
    //     IEventHandler<TMessage> handler, IEventFilter<TMessage> filter1, IEventFilter<TMessage> filter2, IEventFilter<TMessage> filter3) 
    // {
    //     var filters = ArrayPool<IEventFilter<TMessage>>.Shared.Rent(3);
    //     filters[0] = filter1;
    //     filters[1] = filter2;
    //     filters[2] = filter3;
    //
    //     try {
    //         var compositeFilter = CompositeEventFilter<TMessage>.Create(filters.Take(3));
    //         var filteredHandler = new FilteredEventHandler<TMessage>(handler, compositeFilter);
    //         return subscriber.Subscribe(filteredHandler);
    //     }
    //     finally {
    //         ArrayPool<IEventFilter<TMessage>>.Shared.Return(filters);
    //     }
    // }

    /// <summary>
    /// Subscribes to the event with an action and optional filters.
    /// </summary>
    /// <typeparam name="TMessage">The event message type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="handler">The action to invoke on event.</param>
    /// <param name="filters">Optional event filters.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    // public static IDisposable Subscribe<TMessage>(
    //     this IEventSubscriber<TMessage> subscriber,
    //     Action<TMessage> handler,
    //     params IEventFilter<TMessage>[] filters) 
    // {
    //     var filteredHandler = GlobalConfig.FilteredHandlerFactory.CreateFilteredHandler(new AnonymousEventHandler<TMessage>(handler), filters);
    //     return subscriber.Subscribe(filteredHandler);
    // }

    /// <summary>
    /// Subscribes to the event with an action and a predicate filter.
    /// </summary>
    /// <typeparam name="TMessage">The event message type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="handler">The action to invoke on event.</param>
    /// <param name="predicateFilter">Predicate filter for event messages.</param>
    /// <param name="additionalFilters">Additional filters to apply</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    // public static IDisposable Subscribe<TMessage>(
    //     this IEventSubscriber<TMessage> subscriber, 
    //     Action<TMessage> handler,
    //     Func<TMessage, bool> predicateFilter,
    //     params IEventFilter<TMessage>[] additionalFilters) 
    // {
    //     if (additionalFilters.Length == 0) {
    //         return subscriber.Subscribe(
    //             new AnonymousEventHandler<TMessage>(handler),
    //             new PredicateEventFilter<TMessage>(predicateFilter)
    //         );
    //     }
    //     
    //     var filters = ArrayPool<IEventFilter<TMessage>>.Shared.Rent(additionalFilters.Length + 1);
    //     try {
    //         filters[0] = new PredicateEventFilter<TMessage>(predicateFilter);
    //         Array.Copy(additionalFilters, 0, filters, 1, additionalFilters.Length);
    //         var compositeFilter = CompositeEventFilter<TMessage>.Create(filters.Take(additionalFilters.Length + 1));
    //         return subscriber.Subscribe(
    //             new AnonymousEventHandler<TMessage>(handler),
    //             compositeFilter
    //         );
    //     }
    //     finally {
    //         ArrayPool<IEventFilter<TMessage>>.Shared.Return(filters, clearArray: true);
    //     }
    // }
}