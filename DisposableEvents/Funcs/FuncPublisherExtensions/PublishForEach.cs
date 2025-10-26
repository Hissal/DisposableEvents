namespace DisposableEvents;

public static partial class FuncPublisherExtensions {
    public static void InvokeForEach<TArg, TReturn>(this IFuncPublisher<TArg, TReturn> publisher, TArg arg, Action<FuncResult<TReturn>> forEach) {
        foreach (var handler in publisher.GetHandlers()) {
            forEach(publisher.InvokeHandler(handler, arg));
        }
    }
    
    public static void InvokeForEach<TState, TArg, TReturn>(this IFuncPublisher<TArg, TReturn> publisher, TArg arg, TState state, Action<TState, FuncResult<TReturn>> forEach) {
        foreach (var handler in publisher.GetHandlers()) {
            forEach(state, publisher.InvokeHandler(handler, arg));
        }
    }
    
    public static void InvokeForEach<TArg, TReturn>(this IFuncPublisher<TArg, TReturn> publisher, TArg arg, Action<FuncResult<TReturn>, int> forEach) {
        var currentIndex = 0;
        foreach (var handler in publisher.GetHandlers()) {
            forEach(publisher.InvokeHandler(handler, arg), currentIndex);
            currentIndex++;
        }
    }
    
    public static void InvokeForEach<TState, TArg, TReturn>(this IFuncPublisher<TArg, TReturn> publisher, TArg arg, TState state, Action<TState, FuncResult<TReturn>, int> forEach) {
        var currentIndex = 0;
        foreach (var handler in publisher.GetHandlers()) {
            forEach(state, publisher.InvokeHandler(handler, arg), currentIndex);
            currentIndex++;
        }
    }
    
    public static void InvokeForEachValue<TArg, TReturn>(this IFuncPublisher<TArg, TReturn> publisher, TArg arg, Action<TReturn> forEach) {
        foreach (var handler in publisher.GetHandlers()) {
            var result = publisher.InvokeHandler(handler, arg);
            if (result.HasValue) {
                forEach.Invoke(result.Value);
            }
        }
    }
    
    public static void InvokeForEachValue<TState, TArg, TReturn>(this IFuncPublisher<TArg, TReturn> publisher, TArg arg, TState state, Action<TState, TReturn> forEach) {
        foreach (var handler in publisher.GetHandlers()) {
            var result = publisher.InvokeHandler(handler, arg);
            if (result.HasValue) {
                forEach.Invoke(state, result.Value);
            }
        }
    }
    
    public static void InvokeForEachValue<TArg, TReturn>(this IFuncPublisher<TArg, TReturn> publisher, TArg arg, Action<TReturn, int> forEach) {
        var currentIndex = 0;
        foreach (var handler in publisher.GetHandlers()) {
            var result = publisher.InvokeHandler(handler, arg);
            if (result.HasValue) {
                forEach.Invoke(result.Value, currentIndex);
            }
            currentIndex++;
        }
    }
    
    public static void InvokeForEachValue<TState, TArg, TReturn>(this IFuncPublisher<TArg, TReturn> publisher, TArg arg, TState state, Action<TState, TReturn, int> forEach) {
        var currentIndex = 0;
        foreach (var handler in publisher.GetHandlers()) {
            var result = publisher.InvokeHandler(handler, arg);
            if (result.HasValue) {
                forEach.Invoke(state, result.Value, currentIndex);
            }
            currentIndex++;
        }
    }
}