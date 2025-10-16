using System.Runtime.CompilerServices;
using DisposableEvents.Disposables;
using DisposableEvents.Internal;

namespace DisposableEvents;

// TODO: Benchmark keeping count of observers vs iterating always when clearing subscriptions
// TODO: Consider making core implement IDisposableEvent<TMessage> directly to be able to use all events as decorators with the core at the bottom
public sealed class EventCore<TMessage> : IDisposable {
    internal FreeList<IEventHandler<TMessage>> Handlers { get; }
    public bool IsDisposed { get; private set; }
    
    public EventCore() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public EventCore(int initialSubscriberCapacity) {
        Handlers = new FreeList<IEventHandler<TMessage>>(initialSubscriberCapacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Publish(TMessage message) {
        foreach (var handler in Handlers.GetValues()) {
            handler?.Handle(message);
        }
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler) {
        if (IsDisposed) {
            if (handler is IObserver<TMessage> observer)
                observer.OnCompleted();
            return Disposable.Empty;
        }

        var subscriptionKey = Handlers.Add(handler);
        return new Subscription(this, subscriptionKey);
    }

    void Unsubscribe(int subscriptionKey) {
        var handler = Handlers.GetValue(subscriptionKey);
        Handlers.Remove(subscriptionKey, true);
        
        if (handler is IObserver<TMessage> observer)
            observer.OnCompleted();
    }

    public void ClearSubscriptions() {
        if (IsDisposed)
            return;

        var copiedHandlers = Handlers.GetValues();
        Handlers.Clear();
        
        foreach (var handler in copiedHandlers) {
            if (handler is IObserver<TMessage> observer)
                observer.OnCompleted();
        }
    }

    public void Dispose() {
        if (IsDisposed)
            return;
        
        var copiedHandlers = Handlers.GetValues();
        
        Handlers.Dispose();
        IsDisposed = true;
        
        foreach (var handler in copiedHandlers) {
            if (handler is IObserver<TMessage> observer)
                observer.OnCompleted();
        }
    }

    sealed class Subscription : IDisposable {
        bool isDisposed;
        readonly EventCore<TMessage> core;
        readonly int subscriptionKey;

        public Subscription(EventCore<TMessage> core, int subscriptionKey) {
            this.core = core;
            this.subscriptionKey = subscriptionKey;
        }

        public void Dispose() {
            if (isDisposed)
                return;

            isDisposed = true;
            if (core.IsDisposed)
                return;

            core.Unsubscribe(subscriptionKey);
        }
    }
}

public static class EventCoreExtensions {
    public static IEnumerable<IEventHandler<TMessage>> GetHandlers<TMessage>(this EventCore<TMessage> core) {
        return core.Handlers.GetValues().Where(h => h != null)!;
    }
    
    public static int HandlerCount<TMessage>(this EventCore<TMessage> core) {
        return core.Handlers.GetCount();
    }
}