using DisposableEvents.Factories;

namespace DisposableEvents;

public interface IEventHub {
    IEventPublisher<TMessage> GetPublisher<TMessage>();
    IEventSubscriber<TMessage> GetSubscriber<TMessage>();
}

public interface IEventHub<in TMessageRestriction> {
    IEventPublisher<TMessage> GetPublisher<TMessage>() where TMessage : TMessageRestriction;
    IEventSubscriber<TMessage> GetSubscriber<TMessage>() where TMessage : TMessageRestriction;
}

public interface IDisposableEventHub : IEventHub, IDisposable;
public interface IDisposableEventHub<in TMessageRestriction> : IEventHub<TMessageRestriction>, IDisposable;

public sealed class EventHub : IDisposableEventHub {
    readonly EventRegistry registry = new();
    readonly IEventFactory factory;
    
    readonly object gate = new();
    
    bool disposed;
    
    public EventHub() : this(GlobalConfig.EventFactory) { }
    public EventHub(IEventFactory factory) {
        this.factory = factory;
    }

    public IEventPublisher<TMessage> GetPublisher<TMessage>() => GetEvent<TMessage>();
    public IEventSubscriber<TMessage> GetSubscriber<TMessage>() => GetEvent<TMessage>();
    
    IDisposableEvent<TMessage> GetEvent<TMessage>() {
        lock (gate) {
            return disposed 
                ? NullEvent<TMessage>.Instance
                : registry.GetOrAddEvent(factory, f => f.CreateEvent<TMessage>());
        }
    }

    public void Dispose() {
        lock (gate) {
            if (disposed)
                return;
            
            disposed = true;
            registry.Dispose();
        }
    }
}

public sealed class EventHub<TMessageRestriction> : IDisposableEventHub<TMessageRestriction> {
    readonly EventRegistry registry = new();
    readonly IEventFactory factory;
    
    readonly object gate = new();
    
    bool disposed;
    
    public EventHub() : this(GlobalConfig.EventFactory) { }
    public EventHub(IEventFactory factory) {
        this.factory = factory;
    }
    
    public IEventPublisher<TMessage> GetPublisher<TMessage>() where TMessage : TMessageRestriction => GetEvent<TMessage>();
    public IEventSubscriber<TMessage> GetSubscriber<TMessage>() where TMessage : TMessageRestriction => GetEvent<TMessage>();

    IDisposableEvent<TMessage> GetEvent<TMessage>() where TMessage : TMessageRestriction {
        lock (gate) {
            return disposed 
                ? NullEvent<TMessage>.Instance
                : registry.GetOrAddEvent(factory, f => f.CreateEvent<TMessage>());
        }
    }

    public void Dispose() {
        lock (gate) {
            if (disposed)
                return;
            
            disposed = true;
            registry.Dispose();
        }
    }
}
