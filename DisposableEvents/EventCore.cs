using DisposableEvents.Disposables;
using DisposableEvents.Internal;

namespace DisposableEvents;

public class EventCore<TMessage> : IDisposable {
    public List<IObserver<TMessage>> Observers { get; }
    public bool IsDisposed { get; private set; }
    public IObserver<TMessage>[] GetObservers() => Observers.ToArray();
    

    public EventCore(int expectedSubscriptionCount = 2) {
        Observers = new List<IObserver<TMessage>>(expectedSubscriptionCount);
    }
    
    
    public void Publish(TMessage value) {
        foreach (var observer in Observers) {
            try { 
                observer.OnNext(value);
            }
            catch (Exception e) { 
                observer.OnError(e);
            }
        }
    }
    
    public IDisposable Subscribe(IObserver<TMessage> observer) {
        ThrowHelper.ThrowIfNull(observer);
        
        if (IsDisposed) {
            observer.OnCompleted();
            return Disposable.Empty;
        }
        
        Observers.Add(observer);
        var subscription = new Subscription(this, observer);
        return subscription;
    }
    
    ~EventCore() {
        Dispose();
    }
    
    public void Dispose() {
        if (IsDisposed) return;
        
        foreach (var observer in Observers) {
            observer.OnCompleted();
        }
        
        Observers.Clear();

        IsDisposed = true;
        GC.SuppressFinalize(this);
    }
    
    sealed class Subscription : IDisposable {
        bool isDisposed;
        readonly EventCore<TMessage> core;
        readonly IObserver<TMessage> observer;

        public Subscription(EventCore<TMessage> eventInstance, IObserver<TMessage> observer) {
            core = eventInstance ?? throw new ArgumentNullException(nameof(eventInstance));
            this.observer = observer ?? throw new ArgumentNullException(nameof(observer));
        }

        public void Dispose() {
            if (isDisposed) return;
            isDisposed = true;
            
            if (core.IsDisposed) return;
            core.Observers.Remove(observer);
        }
    }
}