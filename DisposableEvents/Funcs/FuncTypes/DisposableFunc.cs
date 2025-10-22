namespace DisposableEvents;

public sealed class DisposableFunc<TMessage, TReturn> : IDisposableFunc<TMessage, TReturn> {
    readonly FuncCore<TMessage, TReturn> core;

    public DisposableFunc() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public DisposableFunc(int initialSubscriberCapacity) {
        core = new FuncCore<TMessage, TReturn>(initialSubscriberCapacity);
    }
    
    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;

    public FuncResult<TReturn> Publish(TMessage message) => core.Publish(message);
    public IDisposable Subscribe(IFuncHandler<TMessage, TReturn> handler) => core.Subscribe(handler);
    
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
    
    FuncResult<TReturn> IFuncPublisher<TMessage, TReturn>.PublishTo(IFuncHandler<TMessage, TReturn> handler, TMessage message) => core.PublishTo(handler, message);
    IFuncHandler<TMessage, TReturn>[] IFuncPublisher<TMessage, TReturn>.GetHandlers() => core.GetHandlers();
}

public sealed class DisposableFunc<TReturn> : IDisposableFunc<Void, TReturn> {
    readonly FuncCore<Void, TReturn> core;

    public DisposableFunc() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public DisposableFunc(int initialSubscriberCapacity) {
        core = new FuncCore<Void, TReturn>(initialSubscriberCapacity);
    }
    
    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;

    public FuncResult<TReturn> Publish(Void message) => core.Publish(message);
    public IDisposable Subscribe(IFuncHandler<Void, TReturn> handler) => core.Subscribe(handler);
    
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();

    FuncResult<TReturn> IFuncPublisher<Void, TReturn>.PublishTo(IFuncHandler<Void, TReturn> handler, Void message) => core.PublishTo(handler, message);
    IFuncHandler<Void, TReturn>[] IFuncPublisher<Void, TReturn>.GetHandlers() => core.GetHandlers();
}