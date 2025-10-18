using System.Runtime.CompilerServices;
using DisposableEvents.Disposables;
using DisposableEvents.Internal;

namespace DisposableEvents;

public sealed class FuncCore<TMessage, TReturn> : IDisposableFunc<TMessage, TReturn> {
    internal readonly FreeList<IFuncHandler<TMessage, TReturn>> Handlers;
    
    bool disposed;
    public bool IsDisposed {
        get {
            lock (gate) {
                return disposed;
            }
        }
    }

    readonly object gate = new();
    
    public int SubscriberCount => Handlers.GetCount();
    
    IFuncHandler<TMessage, TReturn>[]? cachedHandlers;
    public IFuncHandler<TMessage, TReturn>[] GetHandlers() {
        if (cachedHandlers != null)
            return cachedHandlers;
        
        cachedHandlers = Handlers.GetValues().Where(h => h != null).ToArray()!;
        return cachedHandlers;
    }

    public FuncCore() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public FuncCore(int initialSubscriberCapacity) {
        Handlers = new FreeList<IFuncHandler<TMessage, TReturn>>(initialSubscriberCapacity);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FuncResult<TReturn> PublishTo(IFuncHandler<TMessage, TReturn> handler, TMessage message) {
        return handler.Handle(message);
    }
    
    public FuncResult<TReturn> Publish(TMessage message) {
        var result = FuncResult<TReturn>.Null();
        
        foreach (var handler in Handlers.GetValues()) {
            if (handler != null) {
                result = PublishTo(handler, message);
            }
        }
        
        return result;
    }
    
    public IDisposable Subscribe(IFuncHandler<TMessage, TReturn> handler) {
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
        readonly FuncCore<TMessage, TReturn> core;
        readonly int subscriptionKey;

        public Subscription(FuncCore<TMessage, TReturn> core, int subscriptionKey) {
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