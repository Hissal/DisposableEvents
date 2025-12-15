using System.Runtime.CompilerServices;
using DisposableEvents.Disposables;
using DisposableEvents.Internal;

namespace DisposableEvents;

public sealed class FuncCore<TArg, TResult> : AbstractFuncSubscriber<TArg, TResult>, IDisposableFunc<TArg, TResult> {
    internal readonly FreeList<IFuncHandler<TArg, TResult>> Handlers;
    
    bool disposed;
    public bool IsDisposed {
        get {
            lock (gate) {
                return disposed;
            }
        }
    }

    readonly object gate = new();
    
    public int HandlerCount => Handlers.GetCount();
    
    PooledArray<IFuncHandler<TArg, TResult>>? pooledHandlers;
    
    public FuncCore() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public FuncCore(int initialSubscriberCapacity) {
        Handlers = new FreeList<IFuncHandler<TArg, TResult>>(initialSubscriberCapacity);
    }
    
    ReadOnlySpan<IFuncHandler<TArg, TResult>> GetHandlersSpan() {
        lock (gate) {
            if (disposed || HandlerCount == 0)
                return ReadOnlySpan<IFuncHandler<TArg, TResult>>.Empty;
            
            if (pooledHandlers != null)
                return pooledHandlers.Value.Span;
        
            var buffer = PooledArray<IFuncHandler<TArg, TResult>>.Rent(HandlerCount);
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
    
    public FuncHandlerSnapshot<TArg, TResult> SnapshotHandlers() {
        lock (gate) {
            return new FuncHandlerSnapshot<TArg, TResult>(GetHandlersSpan());
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public FuncResult<TResult> InvokeHandler(IFuncHandler<TArg, TResult> handler, TArg arg) {
        return handler.Handle(arg);
    }
    
    public FuncResult<TResult> Invoke(TArg arg) {
        var result = FuncResult<TResult>.Null();
        
        foreach (var handler in Handlers.GetValues()) {
            if (handler != null) {
                result = InvokeHandler(handler, arg);
            }
        }
        
        return result;
    }
    
    public override IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler) {
        lock (gate) {
            if (disposed)
                return Disposable.Empty;

            DisposePooledHandlers();
            
            var subscriptionKey = Handlers.Add(handler);
            return new Subscription(this, subscriptionKey);
        }
    }

    public void ClearHandlers() {
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
        readonly FuncCore<TArg, TResult> core;
        readonly int subscriptionKey;

        public Subscription(FuncCore<TArg, TResult> core, int subscriptionKey) {
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