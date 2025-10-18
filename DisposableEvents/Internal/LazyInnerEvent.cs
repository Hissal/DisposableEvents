using DisposableEvents.Disposables;

namespace DisposableEvents;

internal class LazyInnerEvent<TMessage> : IDisposableEvent<TMessage>, IPipelineEvent<TMessage> {
    readonly int expectedSubscriberCount;
    readonly object gate = new();

    IDisposableEvent<TMessage>? coreLazy;
    bool frozen; // true once inner is set or first used
    bool disposed; // true after Dispose, even if never materialized

    IDisposableEvent<TMessage> Core {
        get {
            var existing = Volatile.Read(ref coreLazy);
            if (existing != null) {
                Volatile.Write(ref frozen, true);
                return existing;
            }

            lock (gate) {
                coreLazy ??= new EventCore<TMessage>(expectedSubscriberCount);
                frozen = true;
                return coreLazy;
            }
        }
    }

    public bool IsDisposed {
        get {
            if (Volatile.Read(ref disposed))
                return true;

            var existing = Volatile.Read(ref coreLazy);
            var isDisposed = existing?.IsDisposed ?? false;
            if (isDisposed)
                Volatile.Write(ref disposed, true);
            return isDisposed;
        }
    }
    
    public LazyInnerEvent(int expectedSubscriberCount = -1) {
        this.expectedSubscriberCount = expectedSubscriberCount > 0 
            ? expectedSubscriberCount 
            : GlobalConfig.InitialSubscriberCapacity;
    }

    public void Publish(TMessage message) {
        if (Volatile.Read(ref disposed))
            return;

        var inner = Volatile.Read(ref coreLazy);
        inner?.Publish(message);
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler) {
        return Volatile.Read(ref disposed)
            ? Disposable.Empty
            : Core.Subscribe(handler);
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter) {
        return Volatile.Read(ref disposed)
            ? Disposable.Empty
            : Core.Subscribe(handler, filter);
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage>[] filters,
        FilterOrdering ordering) {
        return Volatile.Read(ref disposed)
            ? Disposable.Empty
            : Core.Subscribe(handler, filters, ordering);
    }

    public void ClearSubscriptions() {
        if (IsDisposed)
            return;

        var existing = Volatile.Read(ref coreLazy);
        existing?.ClearSubscriptions();
    }

    public void Dispose() {
        lock (gate) {
            if (disposed) 
                return;
            
            disposed = true;
            frozen = true;

            var existing = coreLazy;
            coreLazy = null;
            existing?.Dispose();
        }
    }

    public IPipelineEvent<TMessage>? Next {
        get {
            var existing = Volatile.Read(ref coreLazy);
            if (existing is IPipelineEvent<TMessage> pipelineEvent)
                return pipelineEvent;

            return null;
        }
    }
    public void SetNext(IPipelineEvent<TMessage> next) {
        lock (gate) {
            if (disposed)
                throw new ObjectDisposedException(nameof(LazyInnerEvent<TMessage>));

            if (frozen)
                throw new InvalidOperationException("Inner already materialized; cannot set after first use.");

            coreLazy = next;
            frozen = true;
        }
    }
}