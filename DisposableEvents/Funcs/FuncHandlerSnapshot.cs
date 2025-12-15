using DisposableEvents.Internal;

namespace DisposableEvents;

public ref struct FuncHandlerSnapshot<TArg, TResult> : IDisposable {
    PooledArray<IFuncHandler<TArg, TResult>> pooledHandlers;
    
    public int Length => pooledHandlers.Length;
    public IFuncHandler<TArg, TResult> this[int index] => pooledHandlers[index];
    
    public ReadOnlySpan<IFuncHandler<TArg, TResult>> Span => pooledHandlers.Span;
    
    public FuncHandlerSnapshot(ReadOnlySpan<IFuncHandler<TArg, TResult>> handlers) {
        if (handlers.Length == 0) {
            pooledHandlers = PooledArray<IFuncHandler<TArg, TResult>>.Disposed;
            return;
        }
            
        var buffer = PooledArray<IFuncHandler<TArg, TResult>>.Rent(handlers.Length);
        handlers.CopyTo(buffer.Span);
        pooledHandlers = buffer;
    }
    
    public void Dispose() {
        if (pooledHandlers.IsDisposed)
            return;
        
        PooledArray<IFuncHandler<TArg, TResult>>.Return(ref pooledHandlers, true);
    }
    
    public ReadOnlySpan<IFuncHandler<TArg, TResult>>.Enumerator GetEnumerator() => Span.GetEnumerator();
}