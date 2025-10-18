namespace DisposableEvents;

public interface IFuncMarker;

public interface IFuncPublisher<TMessage, TReturn> : IDisposable {
    bool IsDisposed { get; }
    int SubscriberCount { get; }
    
    /// <summary>
    /// Publishes a message to the event.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <returns>The result of the function call.</returns>
    FuncResult<TReturn> Publish(TMessage message);

    protected internal FuncResult<TReturn> PublishTo(IFuncHandler<TMessage, TReturn> handler, TMessage message);
    protected internal IFuncHandler<TMessage, TReturn>[] GetHandlers();
}

public interface IFuncSubscriber<TMessage, TReturn> {
    IDisposable Subscribe(IFuncHandler<TMessage, TReturn> handler);

    IDisposable Subscribe(IFuncHandler<TMessage, TReturn> handler, IEventFilter<TMessage> filter) {
        var filteredHandler = GlobalConfig.FilteredFuncHandlerFactory.CreateFilteredHandler(handler, filter);
        return Subscribe(filteredHandler);
    }
    
    IDisposable Subscribe(IFuncHandler<TMessage, TReturn> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering) {
        var filteredHandler = GlobalConfig.FilteredFuncHandlerFactory.CreateFilteredHandler(handler, filters, ordering);
        return Subscribe(filteredHandler);
    }
}

public interface IDisposableFunc<TMessage, TReturn> : IFuncPublisher<TMessage, TReturn>, IFuncSubscriber<TMessage, TReturn>, IFuncMarker {
    void ClearSubscriptions();
}

public sealed class DisposableFunc<TMessage, TReturn> : IDisposableFunc<TMessage, TReturn> {
    readonly FuncCore<TMessage, TReturn> core;

    public DisposableFunc() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public DisposableFunc(int initialSubscriberCapacity) {
        core = new FuncCore<TMessage, TReturn>(initialSubscriberCapacity);
    }
    
    public bool IsDisposed => core.IsDisposed;
    public int SubscriberCount => core.SubscriberCount;

    public FuncResult<TReturn> Publish(TMessage message) => core.Publish(message);
    public IDisposable Subscribe(IFuncHandler<TMessage, TReturn> handler) => core.Subscribe(handler);
    
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
    
    FuncResult<TReturn> IFuncPublisher<TMessage, TReturn>.PublishTo(IFuncHandler<TMessage, TReturn> handler, TMessage message) => core.PublishTo(handler, message);
    IFuncHandler<TMessage, TReturn>[] IFuncPublisher<TMessage, TReturn>.GetHandlers() => core.GetHandlers();
}