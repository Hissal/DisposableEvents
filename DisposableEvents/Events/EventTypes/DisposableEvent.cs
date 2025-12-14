
namespace DisposableEvents;

public sealed class DisposableEvent<TMessage> : AbstractSubscriber<TMessage>, IDisposableEvent<TMessage> {
    readonly EventCore<TMessage> core;
    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;
    
    public DisposableEvent() : this(new EventCore<TMessage>()) { }
    public DisposableEvent(int initialSubscriberCapacity) :
        this(new EventCore<TMessage>(initialSubscriberCapacity)) { }

    DisposableEvent(EventCore<TMessage> core) {
        this.core = core;
    }
    
    public override IDisposable Subscribe(IEventHandler<TMessage> handler) => core.Subscribe(handler);
    public void Publish(TMessage message) => core.Publish(message);
    public IEventHandler<TMessage>[] GetHandlers() => core.GetHandlers();
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
}

public sealed class DisposableEvent : AbstractSubscriber<Void>, IDisposableEvent<Void> {
    readonly EventCore<Void> core;
    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;

    public DisposableEvent() : this(new EventCore<Void>()) { }
    public DisposableEvent(int expectedSubscriberCount) :
        this(new EventCore<Void>(expectedSubscriberCount)) { }

    DisposableEvent(EventCore<Void> core) {
        this.core = core;
    }
    
    public override IDisposable Subscribe(IEventHandler<Void> handler) => core.Subscribe(handler);
    public void Publish(Void message) => core.Publish(message);
    public IEventHandler<Void>[] GetHandlers() => core.GetHandlers();
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
}