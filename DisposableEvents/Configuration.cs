using DisposableEvents.Factories;

namespace DisposableEvents;

internal static class GlobalConfig {
    public static int InitialSubscriberCapacity => DisposableEvents.Config.InitialSubscriberCapacity;
    public static IFilteredEventHandlerFactory FilteredHandlerFactory => DisposableEvents.Config.FilteredEventHandlerFactory;
    public static IFilteredFuncHandlerFactory FilteredFuncHandlerFactory => DisposableEvents.Config.FilteredFuncHandlerFactory;
    
    public static IEventFactory EventFactory => DisposableEvents.Config.EventFactory;
}

public sealed class DisposableEventsConfig {
    public static DisposableEventsConfig Create() => new DisposableEventsConfig();

    int initialSubscriberCapacity = 4;
    public int InitialSubscriberCapacity {
        get => initialSubscriberCapacity;
        set {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Initial subscriber capacity must be non-negative.");
            initialSubscriberCapacity = value;
        }
    }

    public IFilteredEventHandlerFactory FilteredEventHandlerFactory { get; set; } = Factories.FilteredEventHandlerFactory.Default;

    public IFilteredFuncHandlerFactory FilteredFuncHandlerFactory { get; set; } = Factories.FilteredFuncHandlerFactory.Default;
    
    public IEventFactory EventFactory { get; set; } = Factories.EventFactory.Default;
}

public static class DisposableEvents {
    public static DisposableEventsConfig Config { get; private set; } = DisposableEventsConfig.Create();

    public static void Configure(Action<DisposableEventsConfig> configure) {
        var config = DisposableEventsConfig.Create();
        configure.Invoke(config);
        Config = config;
    }
}