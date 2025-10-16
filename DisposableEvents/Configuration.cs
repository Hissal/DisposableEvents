using DisposableEvents.Factories;

namespace DisposableEvents;

internal static class GlobalConfig {
    public static int InitialSubscriberCapacity => DisposableEvents.Config.InitialSubscriberCapacity;
    public static IFilteredEventHandlerFactory FilteredHandlerFactory => DisposableEvents.Config.FilteredHandlerFactory;
}

public sealed class DisposableEventsConfig {
    public static DisposableEventsConfig Create() => new DisposableEventsConfig();
    
    public int InitialSubscriberCapacity { get; set; } = 4;
    public IFilteredEventHandlerFactory FilteredHandlerFactory { get; set; } = FilteredEventHandlerFactory.Default;
}

public static class DisposableEvents {
    public static DisposableEventsConfig Config { get; private set; } = DisposableEventsConfig.Create();

    public static void Configure(Action<DisposableEventsConfig> configure) {
        var config = DisposableEventsConfig.Create();
        configure.Invoke(config);
        Config = config;
    }
}