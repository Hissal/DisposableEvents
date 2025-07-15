namespace DisposableEvents.Factories;

public interface IEventFactory {
    IEvent<TMessage> Create<TMessage>();
}

public class EventFactory : IEventFactory {
    public static EventFactory Default { get; } = new EventFactory();

    public IEvent<TMessage> Create<TMessage>() => new Event<TMessage>();
}

public interface IKeyedEventFactory {
    IKeyedEvent<TKey, TMessage> Create<TKey, TMessage>() where TKey : notnull;
}
public class KeyedEventFactory : IKeyedEventFactory {
    public static KeyedEventFactory Default { get; } = new KeyedEventFactory();
    
    public IKeyedEvent<TKey, TMessage> Create<TKey, TMessage>() where TKey : notnull => new KeyedEvent<TKey, TMessage>();
}