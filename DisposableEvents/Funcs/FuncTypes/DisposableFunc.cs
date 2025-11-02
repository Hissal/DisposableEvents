namespace DisposableEvents;

public sealed class DisposableFunc<TArg, TResult> : IDisposableFunc<TArg, TResult> {
    readonly FuncCore<TArg, TResult> core;

    public DisposableFunc() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public DisposableFunc(int initialSubscriberCapacity) {
        core = new FuncCore<TArg, TResult>(initialSubscriberCapacity);
    }
    
    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;

    public FuncResult<TResult> Invoke(TArg arg) => core.Invoke(arg);
    public IDisposable RegisterHandler(IFuncHandler<TArg, TResult> handler) => core.RegisterHandler(handler);
    
    public void ClearHandlers() => core.ClearHandlers();
    public void Dispose() => core.Dispose();
    
    FuncResult<TResult> IFuncPublisher<TArg, TResult>.InvokeHandler(IFuncHandler<TArg, TResult> handler, TArg arg) => core.InvokeHandler(handler, arg);
    IFuncHandler<TArg, TResult>[] IFuncPublisher<TArg, TResult>.GetHandlers() => core.GetHandlers();
}

public sealed class DisposableFunc<TResult> : IDisposableFunc<Void, TResult> {
    readonly FuncCore<Void, TResult> core;

    public DisposableFunc() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public DisposableFunc(int initialSubscriberCapacity) {
        core = new FuncCore<Void, TResult>(initialSubscriberCapacity);
    }
    
    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;

    public FuncResult<TResult> Invoke(Void arg) => core.Invoke(arg);
    public IDisposable RegisterHandler(IFuncHandler<Void, TResult> handler) => core.RegisterHandler(handler);
    
    public void ClearHandlers() => core.ClearHandlers();
    public void Dispose() => core.Dispose();

    FuncResult<TResult> IFuncPublisher<Void, TResult>.InvokeHandler(IFuncHandler<Void, TResult> handler, Void arg) => core.InvokeHandler(handler, arg);
    IFuncHandler<Void, TResult>[] IFuncPublisher<Void, TResult>.GetHandlers() => core.GetHandlers();
}