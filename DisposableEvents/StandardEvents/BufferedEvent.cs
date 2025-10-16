using DisposableEvents.Internal;

namespace DisposableEvents;

public sealed class BufferedEvent<TMessage> : IDisposableEvent<TMessage> {
    readonly EventCore<TMessage> core;

    Optional<TMessage> previousMessage;
    
    public BufferedEvent() : this(new EventCore<TMessage>()) { }
    public BufferedEvent(int expectedSubscriberCount) : this(new EventCore<TMessage>(expectedSubscriberCount)) { }
    BufferedEvent(EventCore<TMessage> core) {
        this.core = core;
        previousMessage = new Optional<TMessage>();
    }

    public void Publish(TMessage message) {
        previousMessage.SetValue(message);
        core.Publish(message);
    }
    public IDisposable Subscribe(IEventHandler<TMessage> handler) {
        if (!core.IsDisposed && previousMessage.TryGet(out var message))
            handler.Handle(message);

        return core.Subscribe(handler);
    }

    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void ClearBufferedMessage() => previousMessage.Clear();
    public void Dispose() => core.Dispose();
}