namespace DisposableEvents;

public class BufferedEvent<TMessage> : IEvent<TMessage> {
    readonly EventCore<TMessage> core = new();
    TMessage? previousMessage;
    
    static readonly bool IsValueType = typeof(TMessage).IsValueType;
    
    public BufferedEvent(int expectedSubscriberCount = 2) : this(new EventCore<TMessage>(expectedSubscriberCount)) { }
    public BufferedEvent(EventCore<TMessage> core) {
        this.core = core;
        previousMessage = default;
    }
    
    public void Publish(TMessage message) {
        previousMessage = message;
        core.Publish(message);
    }
    public IDisposable Subscribe(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) {
        if (filters.Length == 0) {
            return BufferedSubscribe(observer);
        }
        
        var filteredObserver = new FilteredEventObserver<TMessage>(observer, new MultiEventFilter<TMessage>(filters));
        return BufferedSubscribe(filteredObserver);
    }
    
    IDisposable BufferedSubscribe(IObserver<TMessage> observer) {
        if (IsValueType || previousMessage != null) {
            observer.OnNext(previousMessage!);
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