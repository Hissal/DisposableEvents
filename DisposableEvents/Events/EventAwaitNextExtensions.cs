namespace DisposableEvents;

public sealed class AsyncTaskEventHandler<TMessage> : IEventHandler<TMessage>, IDisposable {
    readonly IDisposable subscription;
    readonly CancellationToken cancellationToken;
    readonly CancellationTokenRegistration ctr;
    readonly bool callOnce;
    
    TaskCompletionSource<TMessage>? tcs;
    int isDisposed = 0; // 0 = false, 1 = true
    
    public AsyncTaskEventHandler(IEventSubscriber<TMessage> subscriber, CancellationToken cancellationToken, bool callOnce = true) {
        this.cancellationToken = cancellationToken;
        if (cancellationToken.IsCancellationRequested) {
            isDisposed = 1;
            return;
        }
        
        this.callOnce = callOnce;
        
        subscription = subscriber.Subscribe(this);

        if (cancellationToken.CanBeCanceled) {
            ctr = cancellationToken.Register(state => {
                var self = (AsyncTaskEventHandler<TMessage>)state!;
                self.Dispose();
            }, this, useSynchronizationContext: false);
        }
    }

    public void Handle(TMessage message) {
        Interlocked.Exchange(ref tcs, null)?.TrySetResult(message);
        if (callOnce)
            Dispose();
    }
    
    public async Task<TMessage> OnHandleAsync() {
        var localTcs = tcs ??= new TaskCompletionSource<TMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        
        if (isDisposed == 1)
            localTcs.TrySetCanceled(cancellationToken);
        
        return await localTcs.Task.ConfigureAwait(false);
    }

    public void Dispose() {
        if (Interlocked.Exchange(ref isDisposed, 1) == 1)
            return; 
        
        subscription.Dispose();
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
        return await handler.OnHandleAsync().ConfigureAwait(false);
    }
}