namespace DisposableEvents;

public sealed class DisposableFunc<TArg, TReturn> : IDisposableFunc<TArg, TReturn> {
    readonly FuncCore<TArg, TReturn> core;

    public DisposableFunc() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public DisposableFunc(int initialSubscriberCapacity) {
        core = new FuncCore<TArg, TReturn>(initialSubscriberCapacity);
    }
    
    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;

    public FuncResult<TReturn> Invoke(TArg arg) => core.Invoke(arg);
    public IDisposable RegisterCallback(IFuncHandler<TArg, TReturn> handler) => core.RegisterCallback(handler);
    
    public void ClearHandlers() => core.ClearHandlers();
    public void Dispose() => core.Dispose();
    
    FuncResult<TReturn> IFuncPublisher<TArg, TReturn>.InvokeHandler(IFuncHandler<TArg, TReturn> handler, TArg arg) => core.InvokeHandler(handler, arg);
    IFuncHandler<TArg, TReturn>[] IFuncPublisher<TArg, TReturn>.GetHandlers() => core.GetHandlers();
}

public sealed class DisposableFunc<TReturn> : IDisposableFunc<Void, TReturn> {
    readonly FuncCore<Void, TReturn> core;

    public DisposableFunc() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public DisposableFunc(int initialSubscriberCapacity) {
        core = new FuncCore<Void, TReturn>(initialSubscriberCapacity);
    }
    
    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;

    public FuncResult<TReturn> Invoke(Void arg) => core.Invoke(arg);
    public IDisposable RegisterCallback(IFuncHandler<Void, TReturn> handler) => core.RegisterCallback(handler);
    
    public void ClearHandlers() => core.ClearHandlers();
    public void Dispose() => core.Dispose();

    FuncResult<TReturn> IFuncPublisher<Void, TReturn>.InvokeHandler(IFuncHandler<Void, TReturn> handler, Void arg) => core.InvokeHandler(handler, arg);
    IFuncHandler<Void, TReturn>[] IFuncPublisher<Void, TReturn>.GetHandlers() => core.GetHandlers();
}