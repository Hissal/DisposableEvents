namespace DisposableEvents;

public sealed class FirstFunc<TArg, TReturn> : IDisposableFunc<TArg, TReturn> {
    readonly FuncCore<TArg, TReturn> core;

    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;
    
    public FirstFunc() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public FirstFunc(int initialSubscriberCapacity) {
        core = new FuncCore<TArg, TReturn>(initialSubscriberCapacity);
    }
    
    public FuncResult<TReturn> Invoke(TArg arg) {
        var result = FuncResult<TReturn>.Null();
        
        foreach (var handler in core.Handlers.GetValues()) {
            if (handler == null)
                continue;
            
            result = core.InvokeHandler(handler, arg);
            if (result.HasValue)
                return result;
        }
        
        return result;
    }
    public IDisposable RegisterCallback(IFuncHandler<TArg, TReturn> handler) => core.RegisterCallback(handler);
    
    public void ClearHandlers() => core.ClearHandlers();
    public void Dispose() => core.Dispose();
    
    FuncResult<TReturn> IFuncPublisher<TArg, TReturn>.InvokeHandler(IFuncHandler<TArg, TReturn> handler, TArg arg) => core.InvokeHandler(handler, arg);
    IFuncHandler<TArg, TReturn>[] IFuncPublisher<TArg, TReturn>.GetHandlers() => core.GetHandlers();
}
