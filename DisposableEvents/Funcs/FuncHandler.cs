namespace DisposableEvents;

public interface IFuncHandler<in TMessage, TReturn> {
    FuncResult<TReturn> Handle(TMessage value);
}

public sealed class FuncHandler<TMessage, TReturn> : IFuncHandler<TMessage, TReturn> {
    readonly Func<TMessage, FuncResult<TReturn>> handler;
    public FuncHandler(Func<TMessage, FuncResult<TReturn>> handler) => this.handler = handler;
    public FuncResult<TReturn> Handle(TMessage value) => handler.Invoke(value);
}

public sealed class StatefulFuncHandler<TState, TMessage, TReturn> : IFuncHandler<TMessage, TReturn> {
    readonly Func<TState, TMessage, FuncResult<TReturn>> handler;
    readonly TState state;

    public StatefulFuncHandler(TState state, Func<TState, TMessage, FuncResult<TReturn>> handler) {
        this.handler = handler;
        this.state = state;
    }

    public FuncResult<TReturn> Handle(TMessage value) => handler.Invoke(state, value);
}

public sealed class FilteredFuncHandler<TMessage, TReturn> : IFuncHandler<TMessage, TReturn> {
    readonly IFuncHandler<TMessage, TReturn> handler;
    readonly IEventFilter<TMessage> filter;

    public FilteredFuncHandler(IFuncHandler<TMessage, TReturn> handler, IEventFilter<TMessage> filter) {
        this.handler = handler;
        this.filter = filter;
    }

    public FuncResult<TReturn> Handle(TMessage value) {
        return filter.Filter(ref value).Passed
            ? handler.Handle(value)
            : FuncResult<TReturn>.Null();
    }
}

