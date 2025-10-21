namespace DisposableEvents.Factories;

public interface IEventFactory {
    IDisposableEvent<TMessage> Create<TMessage>();
}

public class EventFactory : IEventFactory {
    public static EventFactory Default { get; } = new EventFactory();

    public IDisposableEvent<TMessage> Create<TMessage>() => new DisposableEvent<TMessage>();
}