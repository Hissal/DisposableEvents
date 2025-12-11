namespace DisposableEvents;

public sealed class AsyncTaskEventHandler<TMessage> : IEventHandler<TMessage>, IDisposable {
    readonly IDisposable? subscription;
    readonly CancellationToken cancellationToken;
    readonly CancellationTokenRegistration ctr;
    readonly bool callOnce;
    
    TaskCompletionSource<TMessage>? tcs;
    int isDisposed = 0; // 0 = false, 1 = true

    AsyncTaskEventHandler(
        IEventSubscriber<TMessage> subscriber,
        CancellationToken cancellationToken = default,
        bool callOnce = true,
        IEventFilter<TMessage>? filter = null, 
        IEventFilter<TMessage>[]? filterArray = null,
        FilterOrdering filterOrdering = FilterOrdering.StableSort) 
    {
        this.cancellationToken = cancellationToken;
        this.callOnce = callOnce;

        if (cancellationToken.IsCancellationRequested) {
            isDisposed = 1;
            return;
        }

        if (filter is not null) {
            subscription = subscriber.Subscribe(this, filter);
        } 
        else if (filterArray is not null) {
            subscription = subscriber.Subscribe(this, filterArray, filterOrdering);
        } 
        else {
            subscription = subscriber.Subscribe(this);
        }

        if (cancellationToken.CanBeCanceled) {
            ctr = cancellationToken.Register(state => {
                var self = (AsyncTaskEventHandler<TMessage>)state!;
                self.Dispose();
            }, this, useSynchronizationContext: false);
        }
    }
    
    public AsyncTaskEventHandler(IEventSubscriber<TMessage> subscriber, CancellationToken cancellationToken = default, bool callOnce = true) 
        : this(subscriber, cancellationToken, callOnce, filter: null, filterArray: null) { }
    
    public AsyncTaskEventHandler(
        IEventSubscriber<TMessage> subscriber,
        IEventFilter<TMessage> filter,
        bool callOnce = true,
        CancellationToken cancellationToken = default) 
        : this(subscriber, cancellationToken, callOnce, filter) { }
    
    public AsyncTaskEventHandler(
        IEventSubscriber<TMessage> subscriber,
        IEventFilter<TMessage>[] filters,
        FilterOrdering filterOrdering = FilterOrdering.StableSort,
        bool callOnce = true,
        CancellationToken cancellationToken = default)
        : this(subscriber, cancellationToken, callOnce, filterArray: filters, filterOrdering: filterOrdering) { }

    public void Handle(TMessage message) {
        Interlocked.Exchange(ref tcs, null)?.TrySetResult(message);
        if (callOnce)
            Dispose();
    }
    
    public async Task<TMessage> AwaitNextAsync() {
        while (true) {
            // Check if disposed first with volatile read
            if (Interlocked.CompareExchange(ref isDisposed, 0, 0) == 1) {
                var canceledTcs = new TaskCompletionSource<TMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
                canceledTcs.TrySetCanceled(cancellationToken);
                return await canceledTcs.Task.ConfigureAwait(false);
            }
            
            var currentTcs = tcs;
            if (currentTcs != null)
                return await currentTcs.Task.ConfigureAwait(false);
            
            var newTcs = new TaskCompletionSource<TMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (Interlocked.CompareExchange(ref tcs, newTcs, null) == null)
                return await newTcs.Task.ConfigureAwait(false);
            // else, another thread set tcs, loop again
        }
    }

    public void Dispose() {
        if (Interlocked.Exchange(ref isDisposed, 1) == 1)
            return; 
        
        subscription?.Dispose();
        ctr.Dispose();
        Interlocked.Exchange(ref tcs, null)?.TrySetCanceled(cancellationToken);
    }
}

/// <summary>
/// Provides extension methods for asynchronously awaiting the next event from an event subscriber.
/// </summary>
/// <remarks>
/// These extension methods enable await-style event handling, allowing code to asynchronously wait for the next
/// event to be published without blocking. The subscription is automatically disposed after receiving the event.
/// </remarks>
public static class EventAwaitNextExtensions {
    /// <summary>
    /// Asynchronously waits for the next event to be published to the subscriber.
    /// </summary>
    /// <typeparam name="TMessage">The type of the event message.</typeparam>
    /// <param name="subscriber">The event subscriber to wait for an event from.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the wait operation. When cancelled, the task will be cancelled
    /// and throw an <see cref="OperationCanceledException"/>.
    /// </param>
    /// <returns>
    /// A task that completes when the next event is published, containing the event message.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <remarks>
    /// This method subscribes to the event, waits for the next event to be published, and then automatically
    /// unsubscribes. The subscription is disposed immediately after receiving the event.
    /// </remarks>
    /// <example>
    /// <code>
    /// var myEvent = new Event&lt;string&gt;();
    /// 
    /// // Await the next event
    /// var task = myEvent.AwaitNextAsync();
    /// myEvent.Publish("Hello");
    /// var message = await task; // Returns "Hello"
    /// </code>
    /// </example>
    public static async Task<TMessage> AwaitNextAsync<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        CancellationToken cancellationToken = default) 
    {
        using var handler = new AsyncTaskEventHandler<TMessage>(subscriber, cancellationToken, true);
        return await handler.AwaitNextAsync().ConfigureAwait(false);
    }
    
    /// <summary>
    /// Asynchronously waits for the next event that passes the specified filter to be published to the subscriber.
    /// </summary>
    /// <typeparam name="TMessage">The type of the event message.</typeparam>
    /// <param name="subscriber">The event subscriber to wait for an event from.</param>
    /// <param name="filter">
    /// The filter to apply to incoming events. Only events that pass this filter will complete the task.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the wait operation. When cancelled, the task will be cancelled
    /// and throw an <see cref="OperationCanceledException"/>.
    /// </param>
    /// <returns>
    /// A task that completes when the next event matching the filter is published, containing the event message.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <remarks>
    /// This method subscribes to the event with the specified filter, waits for the next matching event to be published,
    /// and then automatically unsubscribes. Events that do not pass the filter are ignored.
    /// </remarks>
    /// <example>
    /// <code>
    /// var myEvent = new Event&lt;int&gt;();
    /// var filter = new PredicateEventFilter&lt;int&gt;(x => x > 5);
    /// 
    /// // Await the next event that is greater than 5
    /// var task = myEvent.AwaitNextAsync(filter);
    /// myEvent.Publish(3); // Ignored
    /// myEvent.Publish(7); // Matches filter
    /// var value = await task; // Returns 7
    /// </code>
    /// </example>
    public static async Task<TMessage> AwaitNextAsync<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        IEventFilter<TMessage> filter,
        CancellationToken cancellationToken = default)
    {
        using var handler = new AsyncTaskEventHandler<TMessage>(subscriber, filter, true, cancellationToken);
        return await handler.AwaitNextAsync().ConfigureAwait(false);
    }
    
    /// <summary>
    /// Asynchronously waits for the next event that passes all specified filters to be published to the subscriber.
    /// </summary>
    /// <typeparam name="TMessage">The type of the event message.</typeparam>
    /// <param name="subscriber">The event subscriber to wait for an event from.</param>
    /// <param name="filters">
    /// An array of filters to apply to incoming events. Only events that pass all filters will complete the task.
    /// </param>
    /// <param name="filterOrdering">
    /// Specifies how the filters should be ordered before being applied. Default is <see cref="FilterOrdering.StableSort"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the wait operation. When cancelled, the task will be cancelled
    /// and throw an <see cref="OperationCanceledException"/>.
    /// </param>
    /// <returns>
    /// A task that completes when the next event matching all filters is published, containing the event message.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <remarks>
    /// This method subscribes to the event with the specified filters, waits for the next matching event to be published,
    /// and then automatically unsubscribes. Events that do not pass all filters are ignored. The filters are applied
    /// according to the specified <paramref name="filterOrdering"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var myEvent = new Event&lt;int&gt;();
    /// var filters = new IEventFilter&lt;int&gt;[] {
    ///     new PredicateEventFilter&lt;int&gt;(x => x > 5),
    ///     new PredicateEventFilter&lt;int&gt;(x => x % 2 == 0)
    /// };
    /// 
    /// // Await the next event that is greater than 5 AND even
    /// var task = myEvent.AwaitNextAsync(filters);
    /// myEvent.Publish(3);  // Ignored (not > 5)
    /// myEvent.Publish(7);  // Ignored (not even)
    /// myEvent.Publish(10); // Matches both filters
    /// var value = await task; // Returns 10
    /// </code>
    /// </example>
    public static async Task<TMessage> AwaitNextAsync<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        IEventFilter<TMessage>[] filters,
        FilterOrdering filterOrdering = FilterOrdering.StableSort,
        CancellationToken cancellationToken = default)
    {
        using var handler = new AsyncTaskEventHandler<TMessage>(subscriber, filters, filterOrdering, true, cancellationToken);
        return await handler.AwaitNextAsync().ConfigureAwait(false);
    }
    
    /// <summary>
    /// Asynchronously waits for the next event that passes all specified filters to be published to the subscriber.
    /// </summary>
    /// <typeparam name="TMessage">The type of the event message.</typeparam>
    /// <param name="subscriber">The event subscriber to wait for an event from.</param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the wait operation. When cancelled, the task will be cancelled
    /// and throw an <see cref="OperationCanceledException"/>.
    /// </param>
    /// <param name="filters">
    /// A variable number of filters to apply to incoming events. Only events that pass all filters will complete the task.
    /// Filters are applied using <see cref="FilterOrdering.StableSort"/> ordering.
    /// </param>
    /// <returns>
    /// A task that completes when the next event matching all filters is published, containing the event message.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is cancelled via the <paramref name="cancellationToken"/>.
    /// </exception>
    /// <remarks>
    /// This method subscribes to the event with the specified filters, waits for the next matching event to be published,
    /// and then automatically unsubscribes. Events that do not pass all filters are ignored. This overload provides
    /// a convenient params-style syntax for specifying filters along with a cancellation token.
    /// </remarks>
    /// <example>
    /// <code>
    /// var myEvent = new Event&lt;int&gt;();
    /// var cts = new CancellationTokenSource();
    /// 
    /// // Await the next event with multiple filters and cancellation support
    /// var task = myEvent.AwaitNextAsync(
    ///     cts.Token,
    ///     new PredicateEventFilter&lt;int&gt;(x => x > 5),
    ///     new PredicateEventFilter&lt;int&gt;(x => x % 2 == 0)
    /// );
    /// 
    /// myEvent.Publish(10); // Matches both filters
    /// var value = await task; // Returns 10
    /// </code>
    /// </example>
    public static async Task<TMessage> AwaitNextAsync<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        CancellationToken cancellationToken = default,
        params IEventFilter<TMessage>[] filters)
    {
        using var handler = new AsyncTaskEventHandler<TMessage>(subscriber, filters, FilterOrdering.StableSort, true, cancellationToken);
        return await handler.AwaitNextAsync().ConfigureAwait(false);
    }
}