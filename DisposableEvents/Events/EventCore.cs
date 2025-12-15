using System.Diagnostics;
using System.Runtime.CompilerServices;
using DisposableEvents.Disposables;
using DisposableEvents.Internal;

namespace DisposableEvents;

public sealed class EventCore<TMessage> : AbstractSubscriber<TMessage>, IDisposableEvent<TMessage> {
    internal readonly FreeList<IEventHandler<TMessage>> Handlers;
    
    bool disposed;
    public bool IsDisposed {
        get {
            lock (gate) {
                return disposed;
            }
        }
    }

    public int HandlerCount => Handlers.GetCount();

    readonly object gate = new();
    
    PooledArray<IEventHandler<TMessage>>? pooledHandlers;
    
    public EventCore() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public EventCore(int initialSubscriberCapacity) {
        if (initialSubscriberCapacity < 0) {
            Debug.WriteLine($"[DisposableEvents] Warning: Creating event with negative initial subscriber capacity {initialSubscriberCapacity}. Using default value {GlobalConfig.InitialSubscriberCapacity} instead.");
            initialSubscriberCapacity = GlobalConfig.InitialSubscriberCapacity;
        }
        
        Handlers = new FreeList<IEventHandler<TMessage>>(initialSubscriberCapacity);
    }
    
    ReadOnlySpan<IEventHandler<TMessage>> GetHandlersSpan() {
        lock (gate) {
            if (disposed || HandlerCount == 0)
                return ReadOnlySpan<IEventHandler<TMessage>>.Empty;
        
            if (pooledHandlers != null)
                return pooledHandlers.Value.Span;

            var buffer = PooledArray<IEventHandler<TMessage>>.Rent(HandlerCount);
            var count = 0;
            foreach (var handler in Handlers.GetValues()) {
                if (handler != null) {
                    buffer[count++] = handler;
                }
            }
            
            pooledHandlers = buffer;
            return buffer.Span;
        }
    }
    
    void DisposePooledHandlers() {
        lock (gate) {
            if (pooledHandlers == null)
                return;
            
            pooledHandlers.Value.Dispose();
            pooledHandlers = null;
        }
    }
    
    public EventHandlerSnapshot<TMessage> SnapshotHandlers() {
        lock (gate) {
            return new EventHandlerSnapshot<TMessage>(GetHandlersSpan());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Publish(TMessage message) {
        foreach (var handler in Handlers.GetValues()) {
            handler?.Handle(message);
        }
    }

    public override IDisposable Subscribe(IEventHandler<TMessage> handler) {
        lock (gate) {
            if (disposed)
                return Disposable.Empty;

            DisposePooledHandlers();
            
            var subscriptionKey = Handlers.Add(handler);
            return new Subscription(this, subscriptionKey);
        }
    }

    public void ClearSubscriptions() {
        lock (gate) {
            if (disposed)
                return;

            DisposePooledHandlers();
            Handlers.Clear();
        }
    }

    public void Dispose() {
        lock (gate) {
            if (disposed)
                return;

            DisposePooledHandlers();
            Handlers.Dispose();
            disposed = true;
        }
    }

    sealed class Subscription : IDisposable {
        bool isDisposed;
        readonly EventCore<TMessage> core;
        readonly int subscriptionKey;

        public Subscription(EventCore<TMessage> core, int subscriptionKey) {
            this.core = core;
            this.subscriptionKey = subscriptionKey;
        }

        public void Dispose() {
            if (isDisposed)
                return;

            isDisposed = true;
            lock (core.gate) {
                if (core.disposed)
                    return;

                core.DisposePooledHandlers();
                core.Handlers.Remove(subscriptionKey, true);
            }
        }
    }
}