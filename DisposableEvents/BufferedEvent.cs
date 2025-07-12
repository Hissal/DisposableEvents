namespace DisposableEvents;

public class BufferedEvent<TMessage> : IEvent<TMessage> {
    readonly EventCore<TMessage> core = new();
    TMessage previousValue;
    
    public BufferedEvent(TMessage initialBufferedValue) {
        previousValue = initialBufferedValue;
    }
    
    public IDisposable Subscribe(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) {
        if (filters.Length == 0) {
            observer.OnNext(previousValue);
            return core.Subscribe(observer);
        }
        
        var filteredObserver = new FilteredEventObserver<TMessage>(observer, new MultiEventFilter<TMessage>(filters));
        filteredObserver.OnNext(previousValue);
        return core.Subscribe(filteredObserver);
    }
    public IDisposable Subscribe(IObserver<TMessage> observer) {
        observer.OnNext(previousValue);
        return core.Subscribe(observer);
    }
    
    public void Publish(TMessage value) {
        previousValue = value;
        core.Publish(value);
    }

    public void Dispose() {
        core.Dispose();
        GC.SuppressFinalize(this);
    }
}