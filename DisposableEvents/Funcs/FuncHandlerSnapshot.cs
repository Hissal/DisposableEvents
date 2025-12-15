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
    /// <summary>
    /// Gets an empty snapshot with no handlers.
    /// </summary>
    /// <value>An empty <see cref="FuncHandlerSnapshot{TArg, TResult}"/> instance.</value>
    public static FuncHandlerSnapshot<TArg, TResult> Empty => new (ReadOnlySpan<IFuncHandler<TArg, TResult>>.Empty);

    PooledArray<IFuncHandler<TArg, TResult>> pooledHandlers;
    
    /// <summary>
    /// Gets the number of handlers in this snapshot.
    /// </summary>
    /// <value>The count of handlers captured in this snapshot.</value>
    public int Length => pooledHandlers.Length;
    
    /// <summary>
    /// Gets the handler at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the handler to get.</param>
    /// <value>The <see cref="IFuncHandler{TArg, TResult}"/> at the specified index.</value>
    public IFuncHandler<TArg, TResult> this[int index] => pooledHandlers[index];
    
    /// <summary>
    /// Gets a read-only span over the handlers in this snapshot.
    /// </summary>
    /// <value>A <see cref="ReadOnlySpan{T}"/> containing the handlers, or an empty span if this snapshot has been disposed.</value>
    public ReadOnlySpan<IFuncHandler<TArg, TResult>> Span => !pooledHandlers.IsDisposed ? pooledHandlers.Span : ReadOnlySpan<IFuncHandler<TArg, TResult>>.Empty;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FuncHandlerSnapshot{TArg, TResult}"/> struct with the specified handlers.
    /// </summary>
    /// <param name="handlers">A read-only span of handlers to capture in this snapshot.</param>
    /// <remarks>
    /// If the provided span is empty, no pooled resources are allocated.
    /// Otherwise, handlers are copied into a pooled array for safe enumeration.
    /// </remarks>
    public FuncHandlerSnapshot(ReadOnlySpan<IFuncHandler<TArg, TResult>> handlers) {
        if (handlers.Length == 0) {
            pooledHandlers = PooledArray<IFuncHandler<TArg, TResult>>.Disposed;
            return;
        }
            
        var buffer = PooledArray<IFuncHandler<TArg, TResult>>.Rent(handlers.Length);
        handlers.CopyTo(buffer.Span);
        pooledHandlers = buffer;
    }
    
    /// <summary>
    /// Releases the pooled resources used by this snapshot.
    /// </summary>
    /// <remarks>
    /// After disposal, the <see cref="Span"/> property will return an empty span.
    /// This method is idempotent and can be called multiple times safely.
    /// </remarks>
    public void Dispose() {
        if (pooledHandlers.IsDisposed)
            return;
        
        PooledArray<IFuncHandler<TArg, TResult>>.Return(ref pooledHandlers, true);
    }
    
    /// <summary>
    /// Returns an enumerator that iterates through the handlers in this snapshot.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}.Enumerator"/> for the handlers.</returns>
    public ReadOnlySpan<IFuncHandler<TArg, TResult>>.Enumerator GetEnumerator() => Span.GetEnumerator();
}