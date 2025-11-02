namespace DisposableEvents;

public interface IFuncHandler<in TArg, TResult> {
    FuncResult<TResult> Handle(TArg value);
}

public sealed class FuncHandler<TArg, TResult> : IFuncHandler<TArg, TResult> {
    readonly Func<TArg, FuncResult<TResult>> handler;
    public FuncHandler(Func<TArg, FuncResult<TResult>> handler) => this.handler = handler;
    public FuncResult<TResult> Handle(TArg value) => handler.Invoke(value);
}

public sealed class FuncHandler<TState, TArg, TResult> : IFuncHandler<TArg, TResult> {
    readonly Func<TState, TArg, FuncResult<TResult>> handler;
    readonly TState state;

    public FuncHandler(TState state, Func<TState, TArg, FuncResult<TResult>> handler) {
        this.handler = handler;
        this.state = state;
    }

    public FuncResult<TResult> Handle(TArg value) => handler.Invoke(state, value);
}

public sealed class FilteredFuncHandler<TArg, TResult> : IFuncHandler<TArg, TResult> {
    readonly IFuncHandler<TArg, TResult> handler;
    readonly IEventFilter<TArg> filter;

    public FilteredFuncHandler(IFuncHandler<TArg, TResult> handler, IEventFilter<TArg> filter) {
        this.handler = handler;
        this.filter = filter;
    }

    public FuncResult<TResult> Handle(TArg value) {
        return filter.Filter(ref value).Passed
            ? handler.Handle(value)
            : FuncResult<TResult>.Null();
    }
}

// ----- Void Handlers ---- //
public sealed class VoidFuncHandler<TResult> : IFuncHandler<Void, TResult> {
    readonly Func<FuncResult<TResult>> handler;
    public VoidFuncHandler(Func<FuncResult<TResult>> handler) => this.handler = handler;
    public FuncResult<TResult> Handle(Void value) => handler.Invoke();
}

public sealed class VoidFuncHandler<TState, TResult> : IFuncHandler<Void, TResult> {
    readonly Func<TState, FuncResult<TResult>> handler;
    readonly TState state;

    public VoidFuncHandler(TState state, Func<TState, FuncResult<TResult>> handler) {
        this.handler = handler;
        this.state = state;
    }

    public FuncResult<TResult> Handle(Void value) => handler.Invoke(state);
}

