
namespace DisposableEvents;

public sealed class DisposableEvent<TMessage> : IDisposableEvent<TMessage> {
    readonly EventCore<TMessage> core;
    public bool IsDisposed => core.IsDisposed;
    
    public DisposableEvent() : this(new EventCore<TMessage>()) { }
    public DisposableEvent(int expectedSubscriberCount) :
        this(new EventCore<TMessage>(expectedSubscriberCount)) { }

    DisposableEvent(EventCore<TMessage> core) {
        this.core = core;
    }
    
    public IDisposable Subscribe(IEventHandler<TMessage> handler) => core.Subscribe(handler);
    public void Publish(TMessage message) => core.Publish(message);
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
}

public sealed class DisposableEvent : IDisposableEvent<Void> {
    readonly EventCore<Void> core;
    public bool IsDisposed => core.IsDisposed;
    
    public DisposableEvent() : this(new EventCore<Void>()) { }
    public DisposableEvent(int expectedSubscriberCount) :
        this(new EventCore<Void>(expectedSubscriberCount)) { }

    DisposableEvent(EventCore<Void> core) {
        this.core = core;
    }
    
    public IDisposable Subscribe(IEventHandler<Void> handler) => core.Subscribe(handler);
    public void Publish(Void message) => core.Publish(message);
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
}