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

    FuncHandlerSnapshot<TArg, TResult> SnapshotHandlers();
    void ClearHandlers();
  
    // Would require synchronization to implement properly
    // Maybe in the future with a publicly available sync root
    // ReadOnlySpan<IFuncHandler<TArg, TResult>> GetHandlersSpan();
}

public interface IFuncSubscriber<TArg, TResult> {
    IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler);

    IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler, IEventFilter<TArg> filter)
#if NETSTANDARD2_0
        ;
#else
    {
        var filteredHandler = GlobalConfig.FilteredFuncHandlerFactory.CreateFilteredHandler(handler, filter);
        return RegisterHandler(filteredHandler);
    }
#endif
    
    IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler, IEventFilter<TArg>[] filters, FilterOrdering ordering)
#if NETSTANDARD2_0
        ;
#else
    {
        var filteredHandler = GlobalConfig.FilteredFuncHandlerFactory.CreateFilteredHandler(handler, filters, ordering);
        return RegisterHandler(filteredHandler);
    }
#endif
}

public interface IDisposableFunc<TArg, TResult> : IFuncPublisher<TArg, TResult>, IFuncSubscriber<TArg, TResult>, IFuncMarker;