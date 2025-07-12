using DisposableEvents.Disposables;
using DisposableEvents.Internal;

namespace DisposableEvents;

public class EventCore<TMessage> : IDisposable {
    readonly List<IObserver<TMessage>> observers;

    bool isDisposed;

    public EventCore(int expectedSubscriptionCount = 2) {
        observers = new List<IObserver<TMessage>>(expectedSubscriptionCount);
    }
    
    public void Publish(TMessage value) {
        foreach (var observer in observers) {
            try { 
                observer.OnNext(value);
            }
            catch (Exception e) { 
                observer.OnError(e);
            }
        }
    }
    
    public IDisposable Subscribe(IObserver<TMessage> observer) {
        if (isDisposed) {
            observer.OnCompleted();
            return Disposable.Empty;
        }
        
        observers.Add(observer);
        var subscription = new Subscription(this, observer);
        return subscription;
    }
    
    ~EventCore() {
        Dispose();
    }
    
    public void Dispose() {
        if (isDisposed) return;
        
        foreach (var observer in observers) {
            observer.OnCompleted();
        }
        
        observers.Clear();

        isDisposed = true;
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
            
            if (core.isDisposed) return;
            core.observers.Remove(observer);
        }
    }
}