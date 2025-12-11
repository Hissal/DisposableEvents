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
        if (cancellationToken.IsCancellationRequested) {
            isDisposed = 1;
            return;
        }
        
        this.callOnce = callOnce;

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

public static class EventAwaitNextExtensions {
    public static async Task<TMessage> AwaitNextAsync<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        CancellationToken cancellationToken = default) 
    {
        using var handler = new AsyncTaskEventHandler<TMessage>(subscriber, cancellationToken, true);
        return await handler.AwaitNextAsync().ConfigureAwait(false);
    }
    
    public static async Task<TMessage> AwaitNextAsync<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        IEventFilter<TMessage> filter,
        CancellationToken cancellationToken = default)
    {
        using var handler = new AsyncTaskEventHandler<TMessage>(subscriber, filter, true, cancellationToken);
        return await handler.AwaitNextAsync().ConfigureAwait(false);
    }
    
    public static async Task<TMessage> AwaitNextAsync<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        IEventFilter<TMessage>[] filters,
        FilterOrdering filterOrdering = FilterOrdering.StableSort,
        CancellationToken cancellationToken = default)
    {
        using var handler = new AsyncTaskEventHandler<TMessage>(subscriber, filters, filterOrdering, true, cancellationToken);
        return await handler.AwaitNextAsync().ConfigureAwait(false);
    }
    
    public static async Task<TMessage> AwaitNextAsync<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        CancellationToken cancellationToken = default,
        params IEventFilter<TMessage>[] filters)
    {
        using var handler = new AsyncTaskEventHandler<TMessage>(subscriber, filters, FilterOrdering.StableSort, true, cancellationToken);
        return await handler.AwaitNextAsync().ConfigureAwait(false);
    }
}