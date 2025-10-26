namespace DisposableEvents;

public static class GlobalEventHub {
    static bool s_initialized;
    static IEventHub? s_hub;

    static IEventHub Hub {
        get {
            if (!s_initialized || s_hub == null)
                throw new InvalidOperationException("GlobalEventHub Not Initialized.");
            return s_hub;
        }
    }

    public static void Initialize(IEventHub eventHub) {
        if (s_initialized)
            throw new InvalidOperationException("GlobalEventHub Already Initialized.");
        s_hub = eventHub;
        s_initialized = true;
    }

    public static void Initialize(Func<IEventHub> eventHubFactory) {
        if (s_initialized)
            throw new InvalidOperationException("GlobalEventHub Already Initialized.");
        s_hub = eventHubFactory();
        s_initialized = true;
    }

    public static IEventPublisher<TMessage> GetPublisher<TMessage>() {
        return Hub.GetEvent<TMessage>() ??
               throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the GlobalEventHub.");
    }
    
    public static IEventSubscriber<TMessage> GetSubscriber<TMessage>() {
        return Hub.GetEvent<TMessage>() ??
               throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the GlobalEventHub.");
    }
}

public static class GlobalEventHub<TMessageRestriction> {
    // ReSharper disable once StaticMemberInGenericType
    static bool s_initialized;
    static IEventHub<TMessageRestriction>? s_hub;

    static IEventHub<TMessageRestriction> Hub {
        get {
            if (!s_initialized || s_hub == null)
                throw new InvalidOperationException("GlobalEventHub Not Initialized.");
            return s_hub;
        }
    }

    public static void Initialize(IEventHub<TMessageRestriction> eventHub) {
        if (s_initialized)
            throw new InvalidOperationException("GlobalEventHub Already Initialized.");
        s_hub = eventHub;
        s_initialized = true;
    }

    public static void Initialize(Func<IEventHub<TMessageRestriction>> eventHubFactory) {
        if (s_initialized)
            throw new InvalidOperationException("GlobalEventHub Already Initialized.");
        s_hub = eventHubFactory();
        s_initialized = true;
    }

    public static IEventPublisher<TMessage> GetPublisher<TMessage>() where TMessage : TMessageRestriction {
        return Hub.GetEvent<TMessage>() ??
               throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the GlobalEventHub.");
    }
    
    public static IEventSubscriber<TMessage> GetSubscriber<TMessage>() where TMessage : TMessageRestriction {
        return Hub.GetEvent<TMessage>() ??
               throw new InvalidOperationException($"No event of type {typeof(TMessage)} is registered in the GlobalEventHub.");
    }
}