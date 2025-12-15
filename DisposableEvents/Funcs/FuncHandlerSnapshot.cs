using DisposableEvents.Internal;

namespace DisposableEvents;

/// <summary>
/// Represents a snapshot of function handlers for a given argument and result type.
/// </summary>
/// <typeparam name="TArg">The type of the argument passed to the handlers.</typeparam>
/// <typeparam name="TResult">The type of the result returned by the handlers.</typeparam>
/// <remarks>
/// <para>
/// This struct captures a snapshot of the current set of function handlers at a point in time.
/// It is intended for scenarios where you need to enumerate or invoke handlers without being affected by subsequent modifications.
/// </para>
/// <para>
/// <b>Disposal requirements:</b> This struct must be disposed after use to release any pooled resources.
/// Failure to dispose will result in resource leaks.
/// </para>
/// <para>
/// <b>Usage pattern:</b> Use this struct in a <c>using</c> statement or call <see cref="Dispose"/> explicitly when done.
/// </para>
/// </remarks>
public ref struct FuncHandlerSnapshot<TArg, TResult> : IDisposable {
    public static FuncHandlerSnapshot<TArg, TResult> Empty => new (ReadOnlySpan<IFuncHandler<TArg, TResult>>.Empty);

    PooledArray<IFuncHandler<TArg, TResult>> pooledHandlers;
    
    public int Length => pooledHandlers.Length;
    public IFuncHandler<TArg, TResult> this[int index] => pooledHandlers[index];
    public ReadOnlySpan<IFuncHandler<TArg, TResult>> Span => !pooledHandlers.IsDisposed ? pooledHandlers.Span : ReadOnlySpan<IFuncHandler<TArg, TResult>>.Empty;
    
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