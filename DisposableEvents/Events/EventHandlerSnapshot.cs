using DisposableEvents.Internal;

namespace DisposableEvents;

/// <summary>
/// Represents a snapshot of event handlers for a given message type.
/// </summary>
/// <remarks>
/// <para>
/// This <c>ref struct</c> provides a read-only, disposable view over a set of <see cref="IEventHandler{TMessage}"/> instances,
/// typically used to enumerate handlers at a specific point in time.
/// </para>
/// <para>
/// <b>Disposal requirements:</b> This struct must be disposed after use to return any pooled resources.
/// Failing to dispose will result in resource leaks.
/// </para>
/// <para>
/// <b>Usage pattern:</b> Obtain an instance (e.g., from an event source), enumerate its handlers via <see cref="Span"/> or <see cref="GetEnumerator"/>,
/// and dispose it when done:
/// <code language="csharp">
/// using (var snapshot = GetSnapshot())
/// {
///     foreach (var handler in snapshot.Span)
///     {
///         handler.Handle(message);
///     }
/// }
/// </code>
/// </para>
/// </remarks>
public ref struct EventHandlerSnapshot<TMessage> : IDisposable {
    /// <summary>
    /// Gets an empty snapshot with no handlers.
    /// </summary>
    /// <value>An empty <see cref="EventHandlerSnapshot{TMessage}"/> instance.</value>
    public static EventHandlerSnapshot<TMessage> Empty => new (ReadOnlySpan<IEventHandler<TMessage>>.Empty);
    
    PooledArray<IEventHandler<TMessage>> pooledHandlers;
    
    /// <summary>
    /// Gets the number of handlers in this snapshot.
    /// </summary>
    /// <value>The count of handlers captured in this snapshot.</value>
    public int Length => pooledHandlers.Length;
    
    /// <summary>
    /// Gets the handler at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the handler to get.</param>
    /// <value>The <see cref="IEventHandler{TMessage}"/> at the specified index.</value>
    public IEventHandler<TMessage> this[int index] => pooledHandlers[index];
    
    /// <summary>
    /// Gets a read-only span over the handlers in this snapshot.
    /// </summary>
    /// <value>A <see cref="ReadOnlySpan{T}"/> containing the handlers, or an empty span if this snapshot has been disposed.</value>
    public ReadOnlySpan<IEventHandler<TMessage>> Span => !pooledHandlers.IsDisposed ? pooledHandlers.Span : ReadOnlySpan<IEventHandler<TMessage>>.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventHandlerSnapshot{TMessage}"/> struct with the specified handlers.
    /// </summary>
    /// <param name="handlers">A read-only span of handlers to capture in this snapshot.</param>
    /// <remarks>
    /// If the provided span is empty, no pooled resources are allocated.
    /// Otherwise, handlers are copied into a pooled array for safe enumeration.
    /// </remarks>
    public EventHandlerSnapshot(ReadOnlySpan<IEventHandler<TMessage>> handlers) {
        if (handlers.Length == 0) {
            pooledHandlers = PooledArray<IEventHandler<TMessage>>.Disposed;
            return;
        }
            
        var buffer = PooledArray<IEventHandler<TMessage>>.Rent(handlers.Length);
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
        
        PooledArray<IEventHandler<TMessage>>.Return(ref pooledHandlers, true);
    }
    
    /// <summary>
    /// Returns an enumerator that iterates through the handlers in this snapshot.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{T}.Enumerator"/> for the handlers.</returns>
    public ReadOnlySpan<IEventHandler<TMessage>>.Enumerator GetEnumerator() => Span.GetEnumerator();
}