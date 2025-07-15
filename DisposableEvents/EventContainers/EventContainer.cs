using DisposableEvents.Factories;

namespace DisposableEvents.EventContainers;

public interface IUnkeyedEventContainer : IDisposable {
    IEvent<TMessage> RegisterEvent<TMessage>(IEvent<TMessage>? @event = null);
    bool TryGetEvent<TMessage>(out IEvent<TMessage> @event);

    public IDisposable Subscribe<TMessage>(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters);
    public void Publish<TMessage>(TMessage message);
}
public interface IKeyedEventContainer : IDisposable {
    IKeyedEvent<TKey, TMessage> RegisterEvent<TKey, TMessage>(IKeyedEvent<TKey, TMessage>? @event = null) where TKey : notnull;
    bool TryGetEvent<TKey, TMessage>(out IKeyedEvent<TKey, TMessage> @event) where TKey : notnull;

    public IDisposable Subscribe<TKey, TMessage>(TKey key, IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) where TKey : notnull;
    public void Publish<TKey, TMessage>(TKey key, TMessage message) where TKey : notnull;
}
public interface IEventContainer : IUnkeyedEventContainer, IKeyedEventContainer { }

public sealed class EventContainer : IEventContainer {
    readonly IUnkeyedEventContainer unkeyedEventContainer;
    readonly IKeyedEventContainer keyedEventContainerImplementation;

    public EventContainer() : this(eventContainer: null) {}
    public EventContainer(IEventFactory? eventFactory = null, IKeyedEventFactory? keyedEventFactory = null) :
        this(new UnkeyedEventContainer(eventFactory), new KeyedEventContainer(keyedEventFactory)) { }
    public EventContainer(IUnkeyedEventContainer? eventContainer = null, IKeyedEventContainer? keyedEventContainer = null) {
        if (eventContainer == this) throw new InvalidOperationException($"Cannot use self as {nameof(IUnkeyedEventContainer)}");
        if (keyedEventContainer == this) throw new InvalidOperationException($"Cannot use self as {nameof(IKeyedEventContainer)}");
        
        unkeyedEventContainer = eventContainer ?? new UnkeyedEventContainer();
        keyedEventContainerImplementation = keyedEventContainer ?? new KeyedEventContainer();
    }
    
    // Unkeyed
    public IEvent<TMessage> RegisterEvent<TMessage>(IEvent<TMessage>? @event = null) {
        return unkeyedEventContainer.RegisterEvent(@event);
    }
    public bool TryGetEvent<TMessage>(out IEvent<TMessage> @event) {
        return unkeyedEventContainer.TryGetEvent(out @event);
    }
    public IDisposable Subscribe<TMessage>(IObserver<TMessage> observer, IEventFilter<TMessage>[] filters) {
        return unkeyedEventContainer.Subscribe(observer, filters);
    }
    public void Publish<TMessage>(TMessage message) {
        unkeyedEventContainer.Publish(message);
    }

    // Keyed
    public IKeyedEvent<TKey, TMessage> RegisterEvent<TKey, TMessage>(IKeyedEvent<TKey, TMessage>? @event = null) where TKey : notnull {
        return keyedEventContainerImplementation.RegisterEvent(@event);
    }
    public bool TryGetEvent<TKey, TMessage>(out IKeyedEvent<TKey, TMessage> @event) where TKey : notnull {
        return keyedEventContainerImplementation.TryGetEvent(out @event);
    }
    public IDisposable Subscribe<TKey, TMessage>(TKey key, IObserver<TMessage> observer, IEventFilter<TMessage>[] filters) where TKey : notnull {
        return keyedEventContainerImplementation.Subscribe(key, observer, filters);
    }
    public void Publish<TKey, TMessage>(TKey key, TMessage message) where TKey : notnull {
        keyedEventContainerImplementation.Publish(key, message);
    }

    ~EventContainer() {
        Dispose();
    }
    
    public void Dispose() {
        unkeyedEventContainer.Dispose();
        keyedEventContainerImplementation.Dispose();
        
        GC.SuppressFinalize(this);
    }
}

public sealed class UnkeyedEventContainer : IUnkeyedEventContainer {
    readonly Dictionary<Type, IEvent> events;
    readonly IEventFactory factory;

    public UnkeyedEventContainer(IEventFactory? factory = null) {
        events = new Dictionary<Type, IEvent>();
        this.factory = factory ?? EventFactory.Default;
    }
    
    public IEvent<TMessage> RegisterEvent<TMessage>(IEvent<TMessage>? @event = null){
        @event ??= factory.Create<TMessage>();
        
        if (events.ContainsKey(typeof(TMessage)))
            throw new InvalidOperationException($"Event of type {typeof(TMessage).Name} is already registered");
        
        events[typeof(TMessage)] = @event;
        return @event;
    }

    public bool TryGetEvent<TMessage>(out IEvent<TMessage> @event) {
        if (events.TryGetValue(typeof(TMessage), out var e)) {
            @event = (IEvent<TMessage>)e;
            return true;
        }

        @event = null!;
        return false;
    }

    IEvent<TMessage> GetOrRegisterEvent<TMessage>() {
        return TryGetEvent<TMessage>(out var @event)
            ? @event 
            : RegisterEvent<TMessage>();
    }

    public IDisposable Subscribe<TMessage>(IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) => 
        GetOrRegisterEvent<TMessage>().Subscribe(observer, filters);

    public void Publish<TMessage>(TMessage message) {
        if (TryGetEvent<TMessage>(out var @event)) {
            @event.Publish(message);
        }
    }

    ~UnkeyedEventContainer() {
        Dispose();
    }
    
    public void Dispose() {
        foreach (var @event in events.Values) {
            @event.Dispose();
        }
        
        events.Clear();
        GC.SuppressFinalize(this);
    }
}

public sealed class KeyedEventContainer : IKeyedEventContainer {
    readonly Dictionary<Type, IKeyedEvent> events;
    readonly IKeyedEventFactory factory;

    public KeyedEventContainer(IKeyedEventFactory? factory = null) {
        events = new Dictionary<Type, IKeyedEvent>();
        this.factory = factory ?? KeyedEventFactory.Default;
    }
    
    public IKeyedEvent<TKey, TMessage> RegisterEvent<TKey, TMessage>(IKeyedEvent<TKey, TMessage>? @event = null) where TKey : notnull {
        @event ??= factory.Create<TKey, TMessage>();
        
        if (events.ContainsKey(typeof(TMessage)))
            throw new InvalidOperationException($"Event of type {typeof(TMessage).Name} is already registered");
        
        events[typeof(TMessage)] = @event;
        return @event;
    }

    public bool TryGetEvent<TKey, TMessage>(out IKeyedEvent<TKey, TMessage> @event) where TKey : notnull {
        if (events.TryGetValue(typeof(TMessage), out var e)) {
            @event = (IKeyedEvent<TKey, TMessage>)e;
            return true;
        }

        @event = null!;
        return false;
    }

    IKeyedEvent<TKey, TMessage> GetOrRegisterEvent<TKey, TMessage>() where TKey : notnull {
        return TryGetEvent<TKey, TMessage>(out var @event)
            ? @event
            : RegisterEvent<TKey, TMessage>();
    }

    public IDisposable Subscribe<TKey, TMessage>(TKey key, IObserver<TMessage> observer, params IEventFilter<TMessage>[] filters) where TKey : notnull => 
        GetOrRegisterEvent<TKey, TMessage>().Subscribe(key, observer, filters);

    public void Publish<TKey, TMessage>(TKey key, TMessage message) where TKey : notnull {
        if (TryGetEvent<TKey, TMessage>(out var @event)) {
            @event.Publish(key, message);
        }
    }

    ~KeyedEventContainer() {
        Dispose();
    }
    
    public void Dispose() {
        foreach (var @event in events.Values) {
            @event.Dispose();
        }
        
        events.Clear();
        GC.SuppressFinalize(this);
    }
}