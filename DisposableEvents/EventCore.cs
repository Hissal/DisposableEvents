using System.Runtime.CompilerServices;
using DisposableEvents.Disposables;
using DisposableEvents.Internal;

namespace DisposableEvents;

public sealed class EventCore : IDisposable { 
    internal FreeList<IEventHandler> Handlers { get; }
    public bool IsDisposed { get; private set; }
    public IEventHandler[] CopyHandlers() => Handlers.GetValues().Where(v => v != null).ToArray()!;

    public EventCore(int? expectedSubscriptionCount = null) {
        Handlers = new FreeList<IEventHandler>(expectedSubscriptionCount ?? GlobalConfig.InitialSubscriberCapacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Publish() {
        foreach (var handler in Handlers.GetValues()) {
            handler?.Handle();
        }
    }

    public IDisposable Subscribe(IEventHandler handler) {
        if (IsDisposed) {
            handler.OnUnsubscribe();
            return Disposable.Empty;
        }

        var subscriptionKey = Handlers.Add(handler);
        return new Subscription(this, subscriptionKey);
    }

    void Unsubscribe(int subscriptionKey) {
        Handlers.Remove(subscriptionKey, true);
        var handler = Handlers.GetValue(subscriptionKey);
        handler?.OnUnsubscribe();
    }

    public void ClearSubscriptions() {
        if (IsDisposed)
            return;
        
        foreach (var handler in Handlers.GetValues()) {
            handler?.OnUnsubscribe();
        }

        Handlers.Clear();
    }

    public void Dispose() {
        if (IsDisposed)
            return;

        foreach (var handler in Handlers.GetValues()) {
            handler?.OnUnsubscribe();
        }

        Handlers.Dispose();
        IsDisposed = true;
    }

    sealed class Subscription : IDisposable {
        bool isDisposed;
        readonly EventCore core;
        readonly int subscriptionKey;

        public Subscription(EventCore core, int subscriptionKey) {
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

public sealed class EventCore<TMessage> : IDisposable {
    internal FreeList<IEventHandler<TMessage>> Handlers { get; }
    public bool IsDisposed { get; private set; }
    public IEventHandler<TMessage>[] CopyHandlers() => Handlers.GetValues().Where(v => v != null).ToArray()!;

    public EventCore(int? expectedSubscriptionCount = null) {
        Handlers = new FreeList<IEventHandler<TMessage>>(expectedSubscriptionCount ?? GlobalConfig.InitialSubscriberCapacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Publish(TMessage message) {
        foreach (var handler in Handlers.GetValues()) {
            handler?.Handle(message);
        }
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler) {
        if (IsDisposed) {
            handler.OnUnsubscribe();
            return Disposable.Empty;
        }

        var subscriptionKey = Handlers.Add(handler);
        return new Subscription(this, subscriptionKey);
    }

    void Unsubscribe(int subscriptionKey) {
        Handlers.Remove(subscriptionKey, true);
        var handler = Handlers.GetValue(subscriptionKey);
        handler?.OnUnsubscribe();
    }

    public void ClearSubscriptions() {
        if (IsDisposed)
            return;
        
        foreach (var handler in Handlers.GetValues()) {
            handler?.OnUnsubscribe();
        }

        Handlers.Clear();
    }

    public void Dispose() {
        if (IsDisposed)
            return;

        foreach (var handler in Handlers.GetValues()) {
            handler?.OnUnsubscribe();
        }

        Handlers.Dispose();
        IsDisposed = true;
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