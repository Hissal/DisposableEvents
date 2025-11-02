using System.Diagnostics.CodeAnalysis;
using DisposableEvents.Factories;

namespace DisposableEvents;

public sealed class ManualEventHub : IEventHub {
    readonly EventRegistry registry;
    readonly object gate = new();
    
    bool disposed;
    
    public static Builder CreateBuilder() => new Builder();
    
    ManualEventHub(EventRegistry registry) {
        this.registry = registry;
    }
    
    public IEventPublisher<TMessage> GetPublisher<TMessage>() => GetEvent<TMessage>();
    public IEventSubscriber<TMessage> GetSubscriber<TMessage>() => GetEvent<TMessage>();
        
    IDisposableEvent<TMessage> GetEvent<TMessage>() {
        lock (gate) {
            if (disposed)
                return NullEvent<TMessage>.Instance;
        
            return registry.TryGetEvent<TMessage>(out var eventInstance) 
                ? eventInstance 
                : throw new KeyNotFoundException($"No event of type {typeof(TMessage)} is registered in the ManualEventHub.");
        }
    }
    
    public bool TryGetEvent<TMessage>([NotNullWhen(true)] out IDisposableEvent<TMessage>? eventInstance) {
        lock (gate) {
            if (!disposed)
                return registry.TryGetEvent(out eventInstance);
        
            eventInstance = null;
            return false;
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
    
    public sealed class Builder {
        readonly EventRegistry registry = new EventRegistry();
        IEventFactory? factory;
        
        public Builder SetFactory(IEventFactory factory) {
            this.factory = factory;
            return this;
        }
        
        public Builder RegisterEvent<TMessage>() {
            var eventInstance = factory?.CreateEvent<TMessage>() ?? new DisposableEvent<TMessage>();
            registry.RegisterEvent(eventInstance);
            return this;
        }
        
        public Builder RegisterEvent<TMessage>(IDisposableEvent<TMessage> eventInstance) {
            registry.RegisterEvent(eventInstance);
            return this;
        }
        
        public ManualEventHub Build() {
            return new ManualEventHub(registry);
        }
    }

}

public sealed class ManualEventHub<TMessageRestriction> : IEventHub<TMessageRestriction> {
    readonly EventRegistry registry;
    readonly object gate = new();
    
    bool disposed;
    
    public static Builder CreateBuilder() => new Builder();
    
    ManualEventHub(EventRegistry registry) {
        this.registry = registry;
    }
    
    public IEventPublisher<TMessage> GetPublisher<TMessage>() where TMessage : TMessageRestriction => GetEvent<TMessage>();
    public IEventSubscriber<TMessage> GetSubscriber<TMessage>() where TMessage : TMessageRestriction => GetEvent<TMessage>();
    
    IDisposableEvent<TMessage> GetEvent<TMessage>() where TMessage : TMessageRestriction {
        lock (gate) {
            if (disposed)
                return NullEvent<TMessage>.Instance;
        
            return registry.TryGetEvent<TMessage>(out var eventInstance) 
                ? eventInstance 
                : throw new KeyNotFoundException($"No event of type {typeof(TMessage)} is registered in the ManualEventHub.");
        }
    }

    public bool TryGetEvent<TMessage>([NotNullWhen(true)] out IDisposableEvent<TMessage>? eventInstance) where TMessage : TMessageRestriction {
        lock (gate) {
            if (!disposed)
                return registry.TryGetEvent(out eventInstance);
        
            eventInstance = null;
            return false;
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
    
    public sealed class Builder {
        readonly EventRegistry registry = new EventRegistry();
        IEventFactory? factory;
        
        public Builder SetFactory(IEventFactory factory) {
            this.factory = factory;
            return this;
        }
        
        public Builder RegisterEvent<TMessage>() where TMessage : TMessageRestriction {
            var eventInstance = factory?.CreateEvent<TMessage>() ?? new DisposableEvent<TMessage>();
            registry.RegisterEvent(eventInstance);
            return this;
        }
        
        public Builder RegisterEvent<TMessage>(IDisposableEvent<TMessage> eventInstance) where TMessage : TMessageRestriction {
            registry.RegisterEvent(eventInstance);
            return this;
        }
        
        public ManualEventHub<TMessageRestriction> Build() {
            return new ManualEventHub<TMessageRestriction>(registry);
        }
    }
}