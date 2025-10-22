namespace DisposableEvents;

public sealed class FirstFunc<TMessage, TReturn> : IDisposableFunc<TMessage, TReturn> {
    readonly FuncCore<TMessage, TReturn> core;

    public bool IsDisposed => core.IsDisposed;
    public int HandlerCount => core.HandlerCount;
    
    public FirstFunc() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public FirstFunc(int initialSubscriberCapacity) {
        core = new FuncCore<TMessage, TReturn>(initialSubscriberCapacity);
    }
    
    public FuncResult<TReturn> Publish(TMessage message) {
        var result = FuncResult<TReturn>.Null();
        
        foreach (var handler in core.Handlers.GetValues()) {
            if (handler == null)
                continue;
            
            result = core.PublishTo(handler, message);
            if (result.HasValue)
                return result;
        }
        
        return result;
    }
    public IDisposable Subscribe(IFuncHandler<TMessage, TReturn> handler) => core.Subscribe(handler);
    
    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void Dispose() => core.Dispose();
    
    FuncResult<TReturn> IFuncPublisher<TMessage, TReturn>.PublishTo(IFuncHandler<TMessage, TReturn> handler, TMessage message) => core.PublishTo(handler, message);
    IFuncHandler<TMessage, TReturn>[] IFuncPublisher<TMessage, TReturn>.GetHandlers() => core.GetHandlers();
}
