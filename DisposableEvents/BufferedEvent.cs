using DisposableEvents.Factories;
using DisposableEvents.Internal;

namespace DisposableEvents;

public sealed class BufferedEvent<TMessage> : IEvent<TMessage> {
    readonly EventCore<TMessage> core;
    readonly IEventObserverFactory observerFactory;
    
    TMessage? previousMessage;
    
    static readonly bool s_isValueType = typeof(TMessage).IsValueType;
    
    public BufferedEvent(int expectedSubscriberCount = 2, IEventObserverFactory? observerFactory = null) : this(new EventCore<TMessage>(expectedSubscriberCount), observerFactory) { }
    public BufferedEvent(EventCore<TMessage> core, IEventObserverFactory? observerFactory = null) {
        this.core = core;
        this.observerFactory = observerFactory ?? EventObserverFactory.Default;
        previousMessage = default;
    }
    
    public void Publish(TMessage message) {
        previousMessage = message;
        core.Publish(message);
    }
    public IDisposable Subscribe(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) {
        ThrowHelper.ThrowIfNull(observer);
        return BufferedSubscribe(observerFactory.Create(observer, filters));
    }
    
    IDisposable BufferedSubscribe(IObserver<TMessage> observer) {
        if (!core.IsDisposed && (s_isValueType || previousMessage != null)) {
            try {
                observer.OnNext(previousMessage!);
            }
            catch (Exception e) {
                observer.OnError(e);
            }
        }
        
        return core.Subscribe(observer);
    }
    
    ~BufferedEvent() {
        Dispose();
    }

    public void Dispose() {
        core.Dispose();
        GC.SuppressFinalize(this);
    }
}