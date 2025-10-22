namespace DisposableEvents;

public interface IFuncMarker;

public interface IFuncPublisher<TMessage, TReturn> : IDisposable {
    bool IsDisposed { get; }
    int HandlerCount { get; }
    
    /// <summary>
    /// Publishes a message to the event.
    /// </summary>
    /// <param name="message">The message to publish.</param>
    /// <returns>The result of the function call.</returns>
    FuncResult<TReturn> Publish(TMessage message);

    FuncResult<TReturn> PublishTo(IFuncHandler<TMessage, TReturn> handler, TMessage message);
    IFuncHandler<TMessage, TReturn>[] GetHandlers();
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