using System.Diagnostics;
using System.Runtime.CompilerServices;
using DisposableEvents.Disposables;
using DisposableEvents.Internal;

namespace DisposableEvents;

public sealed class EventCore<TMessage> : IDisposableEvent<TMessage> {
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
    
    // TODO: Consider using ImmutableArray or similar for better thread-safety
    // TODO: Consider using pooled arrays to reduce allocations
    IEventHandler<TMessage>[]? cachedHandlers;
    public IEventHandler<TMessage>[] GetHandlers() {
        lock (gate) {
            if (disposed || HandlerCount == 0)
                return Array.Empty<IEventHandler<TMessage>>();
        
            if (cachedHandlers != null)
                return cachedHandlers;
            
            // TODO: benchmark which is faster linq vs manual loop
            cachedHandlers = Handlers.GetValues().Where(h => h != null).ToArray()!;
            
            // cachedHandlers = new IEventHandler<TMessage>[HandlerCount];
            // int i = 0;
            // foreach (var handler in Handlers.GetValues()) {
            //     if (handler != null) {
            //         cachedHandlers[i++] = handler;
            //     }
            // }
            
            return cachedHandlers;
        }
    }
    
    public EventCore() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public EventCore(int initialSubscriberCapacity) {
        if (initialSubscriberCapacity < 0) {
            Debug.WriteLine($"[DisposableEvents] Warning: Creating event with negative initial subscriber capacity {initialSubscriberCapacity}. Using default value {GlobalConfig.InitialSubscriberCapacity} instead.");
            initialSubscriberCapacity = GlobalConfig.InitialSubscriberCapacity;
        }
        
        Handlers = new FreeList<IEventHandler<TMessage>>(initialSubscriberCapacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Publish(TMessage message) {
        foreach (var handler in Handlers.GetValues()) {
            handler?.Handle(message);
        }
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler) {
        lock (gate) {
            if (disposed)
                return Disposable.Empty;

            cachedHandlers = null;
            
            var subscriptionKey = Handlers.Add(handler);
            return new Subscription(this, subscriptionKey);
        }
    }

    public void ClearSubscriptions() {
        lock (gate) {
            if (disposed)
                return;

            cachedHandlers = null;
            Handlers.Clear();
        }
    }

    public void Dispose() {
        lock (gate) {
            if (disposed)
                return;

            cachedHandlers = null;
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

                core.cachedHandlers = null;
                core.Handlers.Remove(subscriptionKey, true);
            }
        }
    }
}