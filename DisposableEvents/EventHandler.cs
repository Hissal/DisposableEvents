namespace DisposableEvents;

public interface IEventHandler<in TMessage> {
    void Handle(TMessage message);
}

public sealed class EventHandler<TMessage> : IEventHandler<TMessage> {
    readonly Action<TMessage> handler;
    public EventHandler(Action<TMessage> handler) => this.handler = handler;
    public void Handle(TMessage message) => handler(message);
}

public sealed class FilteredEventHandler<TMessage> : IEventHandler<TMessage> {
    readonly IEventHandler<TMessage> handler;
    readonly IEventFilter<TMessage> filter;

    public FilteredEventHandler(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter) {
        this.handler = handler;
        this.filter = filter;
    }

    public void Handle(TMessage message) {
        if (filter.Filter(ref message)) {
            handler.Handle(message);
        }
    }
}

public sealed class StatefulEventHandler<TState, TMessage> : IEventHandler<TMessage> {
    readonly Action<TState, TMessage> handler;
    readonly TState state;

    public StatefulEventHandler(TState state, Action<TState, TMessage> handler) {
        this.state = state;
        this.handler = handler;
    }
    
    public void Handle(TMessage message) => handler(state, message);
}

// ----- Void Handlers ----
public sealed class VoidEventHandler : IEventHandler<Void> {
    readonly Action handler;
    public VoidEventHandler(Action handler) => this.handler = handler;
    public void Handle(Void message) => handler();
}

public sealed class StatefulVoidEventHandler<TState> : IEventHandler<Void> {
    readonly Action<TState> handler;
    readonly TState state;

    public StatefulVoidEventHandler(TState state, Action<TState> handler) {
        this.state = state;
        this.handler = handler;
    }
    
    public void Handle(Void message) => handler(state);
}