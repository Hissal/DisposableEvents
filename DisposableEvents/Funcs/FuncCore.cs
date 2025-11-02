using System.Runtime.CompilerServices;
using DisposableEvents.Disposables;
using DisposableEvents.Internal;

namespace DisposableEvents;

public sealed class FuncCore<TArg, TResult> : IDisposableFunc<TArg, TResult> {
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
    
    IFuncHandler<TArg, TResult>[]? cachedHandlers;
    public IFuncHandler<TArg, TResult>[] GetHandlers() {
        if (cachedHandlers != null)
            return cachedHandlers;
        
        cachedHandlers = Handlers.GetValues().Where(h => h != null).ToArray()!;
        return cachedHandlers;
    }

    public FuncCore() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public FuncCore(int initialSubscriberCapacity) {
        Handlers = new FreeList<IFuncHandler<TArg, TResult>>(initialSubscriberCapacity);
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
    
    public IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler) {
        lock (gate) {
            if (disposed)
                return Disposable.Empty;

            cachedHandlers = null;
            
            var subscriptionKey = Handlers.Add(handler);
            return new Subscription(this, subscriptionKey);
        }
    }

    public void ClearHandlers() {
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

                core.cachedHandlers = null;
                core.Handlers.Remove(subscriptionKey, true);
            }
        }
    }
}