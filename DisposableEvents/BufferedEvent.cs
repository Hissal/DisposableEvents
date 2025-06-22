namespace DisposableEvents;

public class BufferedEvent<T> : IEvent<T> {
    readonly EventCore<T> core = new();
    T previousValue;
    
    public BufferedEvent(T initialBufferedValue = default) {
        previousValue = initialBufferedValue;
    }
    
    public IDisposable Subscribe(IObserver<T> observer, params IEventFilter<T>[] filters) {
        if (filters is null || filters.Length == 0) {
            observer.OnNext(previousValue);
            return core.Subscribe(observer);
        }
        
        var filteredObserver = new FilteredEventReceiver<T>(observer, new MultiEventFilter<T>(filters));
        filteredObserver.OnNext(previousValue);
        return core.Subscribe(filteredObserver);
    }
    public IDisposable Subscribe(IObserver<T> observer) {
        observer.OnNext(previousValue);
        return core.Subscribe(observer);
    }
    
    public void Publish(T value) {
        previousValue = value;
        core.Publish(value);
    }

    public void Dispose() {
        core.Dispose();
        GC.SuppressFinalize(this);
    }
}