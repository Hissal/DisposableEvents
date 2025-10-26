namespace DisposableEvents.Factories;

public interface IEventFactory {
    IDisposableEvent<TMessage> CreateEvent<TMessage>();
}

public class EventFactory : IEventFactory {
    public static EventFactory Default { get; } = new EventFactory();

    public IDisposableEvent<TMessage> CreateEvent<TMessage>() => new DisposableEvent<TMessage>();
}