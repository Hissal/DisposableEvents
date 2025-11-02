namespace DisposableEvents;

public interface IFuncMarker;

public interface IFuncPublisher<TArg, TResult> : IDisposable {
    bool IsDisposed { get; }
    int HandlerCount { get; }
    
    /// <summary>
    /// Publishes a message to the event.
    /// </summary>
    /// <param name="arg">The message to publish.</param>
    /// <returns>The result of the function call.</returns>
    FuncResult<TResult> Invoke(TArg arg);

    FuncResult<TResult> InvokeHandler(IFuncHandler<TArg, TResult> handler, TArg arg);
    IFuncHandler<TArg, TResult>[] GetHandlers();
}

public interface IFuncSubscriber<TArg, TResult> {
    IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler);

    IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler, IEventFilter<TArg> filter) {
        var filteredHandler = GlobalConfig.FilteredFuncHandlerFactory.CreateFilteredHandler(handler, filter);
        return RegisterHandler(filteredHandler);
    }
    
    IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler, IEventFilter<TArg>[] filters, FilterOrdering ordering) {
        var filteredHandler = GlobalConfig.FilteredFuncHandlerFactory.CreateFilteredHandler(handler, filters, ordering);
        return RegisterHandler(filteredHandler);
    }
}

public interface IDisposableFunc<TArg, TResult> : IFuncPublisher<TArg, TResult>, IFuncSubscriber<TArg, TResult>, IFuncMarker {
    void ClearHandlers();
}