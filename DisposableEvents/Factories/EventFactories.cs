namespace DisposableEvents.Factories;

public interface IEventFactory {
    IEvent<TMessage> Create<TMessage>();
}

public class EventFactory : IEventFactory {
    static Lazy<EventFactory> LazyEventFactory { get; } = new Lazy<EventFactory>(() => new EventFactory());
    public static EventFactory Default => LazyEventFactory.Value;

    public IEvent<TMessage> Create<TMessage>() => new Event<TMessage>();
}

public interface IKeyedEventFactory {
    IKeyedEvent<TKey, TMessage> Create<TKey, TMessage>() where TKey : notnull;
}
public class KeyedEventFactory : IKeyedEventFactory {
    static Lazy<KeyedEventFactory> LazyEventFactory { get; } = new Lazy<KeyedEventFactory>(() => new KeyedEventFactory());
    public static KeyedEventFactory Default => LazyEventFactory.Value;
    
    public IKeyedEvent<TKey, TMessage> Create<TKey, TMessage>() where TKey : notnull => new KeyedEvent<TKey, TMessage>();
}

public class BufferedEventFactory : IEventFactory {
    public IEvent<TMessage> Create<TMessage>() => new BufferedEvent<TMessage>();
}