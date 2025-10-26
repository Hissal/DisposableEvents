using System.Diagnostics.CodeAnalysis;
using DisposableEvents.Factories;

namespace DisposableEvents;

public interface IEventHub : IDisposable {
    public IDisposableEvent<TMessage>? GetEvent<TMessage>();
    bool TryGetEvent<TMessage>([NotNullWhen(true)] out IDisposableEvent<TMessage>? eventInstance);
}

public interface IEventHub<in TMessageRestriction> : IDisposable {
    IDisposableEvent<TMessage>? GetEvent<TMessage>() where TMessage : TMessageRestriction;
    bool TryGetEvent<TMessage>([NotNullWhen(true)] out IDisposableEvent<TMessage>? eventInstance) where TMessage : TMessageRestriction;
}

public sealed class EventHub : IEventHub {
    readonly EventRegistry registry = new EventRegistry();
    readonly IEventFactory factory;
    
    public EventHub() : this(GlobalConfig.EventFactory) { }
    public EventHub(IEventFactory factory) {
        this.factory = factory;
    }
    
    public IDisposableEvent<TMessage> GetEvent<TMessage>() {
        return registry.GetOrAddEvent(factory, f => f.CreateEvent<TMessage>());
    }

    public bool TryGetEvent<TMessage>([NotNullWhen(true)] out IDisposableEvent<TMessage>? eventInstance) {
        return registry.TryGetEvent(out eventInstance);
    }

    public void Dispose() {
        registry.Dispose();
    }
}

public sealed class EventHub<TMessageRestriction> : IEventHub<TMessageRestriction> {
    readonly EventRegistry registry = new EventRegistry();
    readonly IEventFactory factory;
    
    public EventHub() : this(GlobalConfig.EventFactory) { }
    public EventHub(IEventFactory factory) {
        this.factory = factory;
    }

    public IDisposableEvent<TMessage> GetEvent<TMessage>() where TMessage : TMessageRestriction {
        return registry.GetOrAddEvent(factory, f => f.CreateEvent<TMessage>());
    }
    
    public bool TryGetEvent<TMessage>([NotNullWhen(true)] out IDisposableEvent<TMessage>? eventInstance) where TMessage : TMessageRestriction {
        return registry.TryGetEvent(out eventInstance);
    }

    public void Dispose() {
        registry.Dispose();
    }
}
