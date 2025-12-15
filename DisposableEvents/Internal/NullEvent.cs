using DisposableEvents.Disposables;

namespace DisposableEvents;

internal class NullEvent<TMessage> :
    AbstractSubscriber<TMessage>,
    IDisposableEvent<TMessage>
{
    public static NullEvent<TMessage> Instance { get; } = new NullEvent<TMessage>();
    
    public bool IsDisposed => true;
    public int HandlerCount => 0;
    
    public ReadOnlySpan<IEventHandler<TMessage>> GetHandlers() => ReadOnlySpan<IEventHandler<TMessage>>.Empty;
    
    public void Publish(TMessage message) { }
    
    public override IDisposable Subscribe(IEventHandler<TMessage> handler) => Disposable.Empty;
    
    public void ClearSubscriptions() { }
    public void Dispose() { }
}