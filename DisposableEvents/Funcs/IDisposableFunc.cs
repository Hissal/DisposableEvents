namespace DisposableEvents;

public interface IFuncMarker;

public interface IFuncPublisher<TArg, TReturn> : IDisposable {
    bool IsDisposed { get; }
    int HandlerCount { get; }
    
    /// <summary>
    /// Publishes a message to the event.
    /// </summary>
    /// <param name="arg">The message to publish.</param>
    /// <returns>The result of the function call.</returns>
    FuncResult<TReturn> Invoke(TArg arg);

    FuncResult<TReturn> InvokeHandler(IFuncHandler<TArg, TReturn> handler, TArg arg);
    IFuncHandler<TArg, TReturn>[] GetHandlers();
}

public interface IFuncSubscriber<TArg, TReturn> {
    IDisposable RegisterCallback(IFuncHandler<TArg, TReturn> handler);

    IDisposable RegisterCallback(IFuncHandler<TArg, TReturn> handler, IEventFilter<TArg> filter) {
        var filteredHandler = GlobalConfig.FilteredFuncHandlerFactory.CreateFilteredHandler(handler, filter);
        return RegisterCallback(filteredHandler);
    }
    
    IDisposable RegisterCallback(IFuncHandler<TArg, TReturn> handler, IEventFilter<TArg>[] filters, FilterOrdering ordering) {
        var filteredHandler = GlobalConfig.FilteredFuncHandlerFactory.CreateFilteredHandler(handler, filters, ordering);
        return RegisterCallback(filteredHandler);
    }
}

public interface IDisposableFunc<TArg, TReturn> : IFuncPublisher<TArg, TReturn>, IFuncSubscriber<TArg, TReturn>, IFuncMarker {
    void ClearHandlers();
}