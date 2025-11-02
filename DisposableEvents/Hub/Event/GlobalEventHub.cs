namespace DisposableEvents;

public static class GlobalEventHub {
    static IEventHub? s_hub;

    static IEventHub Hub =>
        s_hub ?? throw new InvalidOperationException("GlobalEventHub not initialized. Call SetHub before using.");

    public static void SetHub(IEventHub eventHub) => s_hub = eventHub;

    public static IEventPublisher<TMessage> GetPublisher<TMessage>() => Hub.GetPublisher<TMessage>();
    public static IEventSubscriber<TMessage> GetSubscriber<TMessage>() => Hub.GetSubscriber<TMessage>();
}

public static class GlobalEventHub<TMessageRestriction> {
    static IEventHub<TMessageRestriction>? s_hub;

    static IEventHub<TMessageRestriction> Hub => 
        s_hub ?? throw new InvalidOperationException($"GlobalEventHub<{typeof(TMessageRestriction).Name}> not initialized. Call SetHub before using.");

    public static void SetHub(IEventHub<TMessageRestriction> eventHub) => s_hub = eventHub;

    public static IEventPublisher<TMessage> GetPublisher<TMessage>() where TMessage : TMessageRestriction =>
        Hub.GetPublisher<TMessage>();

    public static IEventSubscriber<TMessage> GetSubscriber<TMessage>() where TMessage : TMessageRestriction =>
        Hub.GetSubscriber<TMessage>();
}