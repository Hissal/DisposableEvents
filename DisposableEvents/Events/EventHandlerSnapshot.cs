using DisposableEvents.Internal;

namespace DisposableEvents;

public ref struct EventHandlerSnapshot<TMessage> : IDisposable {
    public static EventHandlerSnapshot<TMessage> Empty => new (ReadOnlySpan<IEventHandler<TMessage>>.Empty);
    
    PooledArray<IEventHandler<TMessage>> pooledHandlers;
    
    public int Length => !pooledHandlers.IsDisposed ? pooledHandlers.Length : 0;
    public IEventHandler<TMessage> this[int index] => pooledHandlers.Span[index];

    public EventHandlerSnapshot(ReadOnlySpan<IEventHandler<TMessage>> handlers) {
        if (handlers.Length == 0) {
            pooledHandlers = PooledArray<IEventHandler<TMessage>>.Disposed;
            return;
        }
            
        var buffer = PooledArray<IEventHandler<TMessage>>.Rent(handlers.Length);
        handlers.CopyTo(buffer.Span);
        pooledHandlers = buffer;
    }

    public ReadOnlySpan<IEventHandler<TMessage>> Span => !pooledHandlers.IsDisposed ? pooledHandlers.Span : ReadOnlySpan<IEventHandler<TMessage>>.Empty ;

    public void Dispose() {
        if (pooledHandlers.IsDisposed)
            return;
        
        PooledArray<IEventHandler<TMessage>>.Return(ref pooledHandlers, true);
    }
    
    public ReadOnlySpan<IEventHandler<TMessage>>.Enumerator GetEnumerator() => Span.GetEnumerator();
}