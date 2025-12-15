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
    public static EventHandlerSnapshot<TMessage> Empty => new (ReadOnlySpan<IEventHandler<TMessage>>.Empty);
    
    PooledArray<IEventHandler<TMessage>> pooledHandlers;
    
    public int Length => pooledHandlers.Length;
    public IEventHandler<TMessage> this[int index] => pooledHandlers[index];
    public ReadOnlySpan<IEventHandler<TMessage>> Span => !pooledHandlers.IsDisposed ? pooledHandlers.Span : ReadOnlySpan<IEventHandler<TMessage>>.Empty;

    public EventHandlerSnapshot(ReadOnlySpan<IEventHandler<TMessage>> handlers) {
        if (handlers.Length == 0) {
            pooledHandlers = PooledArray<IEventHandler<TMessage>>.Disposed;
            return;
        }
            
        var buffer = PooledArray<IEventHandler<TMessage>>.Rent(handlers.Length);
        handlers.CopyTo(buffer.Span);
        pooledHandlers = buffer;
    }


    public void Dispose() {
        if (pooledHandlers.IsDisposed)
            return;
        
        PooledArray<IEventHandler<TMessage>>.Return(ref pooledHandlers, true);
    }
    
    public ReadOnlySpan<IEventHandler<TMessage>>.Enumerator GetEnumerator() => Span.GetEnumerator();
}