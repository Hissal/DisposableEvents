using System.Diagnostics.CodeAnalysis;
using DisposableEvents.Factories;

namespace DisposableEvents;

public sealed class ManualEventHub : IEventHub {
    readonly EventRegistry registry;
    
    public static Builder CreateBuilder() => new Builder();
    
    ManualEventHub(EventRegistry registry) {
        this.registry = registry;
    }
        
    public IDisposableEvent<TMessage>? GetEvent<TMessage>() {
        return registry.GetEvent<TMessage>();
    }

    public bool TryGetEvent<TMessage>([NotNullWhen(true)] out IDisposableEvent<TMessage>? eventInstance) {
        return registry.TryGetEvent(out eventInstance);
    }
    
    public void Dispose() {
        registry.Dispose();
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
    
    public static Builder CreateBuilder() => new Builder();
    
    ManualEventHub(EventRegistry registry) {
        this.registry = registry;
    }
    
    public IDisposableEvent<TMessage>? GetEvent<TMessage>() where TMessage : TMessageRestriction {
        return registry.GetEvent<TMessage>();
    }

    public bool TryGetEvent<TMessage>([NotNullWhen(true)] out IDisposableEvent<TMessage>? eventInstance) where TMessage : TMessageRestriction {
        return registry.TryGetEvent(out eventInstance);
    }
    
    public void Dispose() {
        registry.Dispose();
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