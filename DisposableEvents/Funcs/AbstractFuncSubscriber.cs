namespace DisposableEvents;

/// <summary>
/// Provides a base implementation of <see cref="IFuncSubscriber{TArg, TResult}"/> that handles filtered handler registration.
/// Required in netstandard2.0 where default interface methods are not supported.
/// </summary>
/// <typeparam name="TArg"></typeparam>
/// <typeparam name="TResult"></typeparam>
public abstract class AbstractFuncSubscriber<TArg, TResult> : IFuncSubscriber<TArg, TResult> {
    public abstract IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler);
    
    public IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler, IEventFilter<TArg> filter) {
        var filteredHandler = GlobalConfig.FilteredFuncHandlerFactory.CreateFilteredHandler(handler, filter);
        return RegisterHandler(filteredHandler);
    }
    public IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler, IEventFilter<TArg>[] filters, FilterOrdering ordering) {
        var filteredHandler = GlobalConfig.FilteredFuncHandlerFactory.CreateFilteredHandler(handler, filters, ordering);
        return RegisterHandler(filteredHandler);
    }
}