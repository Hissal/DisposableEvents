namespace DisposableEvents;

public sealed class FirstFunc<TArg, TResult> : AbstractFuncSubscriber<TArg, TResult>, IDisposableFunc<TArg, TResult> {
    readonly FuncCore<TArg, TResult> core;

    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;
    
    public FirstFunc() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public FirstFunc(int initialSubscriberCapacity) {
        core = new FuncCore<TArg, TResult>(initialSubscriberCapacity);
    }
    
    public FuncResult<TResult> Invoke(TArg arg) {
        var result = FuncResult<TResult>.Null();
        
        foreach (var handler in core.Handlers.GetValues()) {
            if (handler == null)
                continue;
            
            result = core.InvokeHandler(handler, arg);
            if (result.HasValue)
                return result;
        }
        
        return result;
    }
    
    public override IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler) => core.RegisterHandler(handler);
    
    public FuncHandlerSnapshot<TArg, TResult> SnapshotHandlers() => core.SnapshotHandlers();
    public void ClearHandlers() => core.ClearHandlers();
    public void Dispose() => core.Dispose();
    
    FuncResult<TResult> IFuncPublisher<TArg, TResult>.InvokeHandler(IFuncHandler<TArg, TResult> handler, TArg arg) => core.InvokeHandler(handler, arg);
}
