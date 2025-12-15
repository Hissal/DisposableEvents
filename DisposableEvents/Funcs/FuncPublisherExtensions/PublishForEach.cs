namespace DisposableEvents;

public static partial class FuncPublisherExtensions {
    public static void InvokeForEach<TArg, TResult>(this IFuncPublisher<TArg, TResult> publisher, TArg arg, Action<FuncResult<TResult>> forEach) {
        using var handlerSnapshot = publisher.SnapshotHandlers();
        foreach (var handler in handlerSnapshot.Span) {
            forEach(publisher.InvokeHandler(handler, arg));
        }
    }
    
    public static void InvokeForEach<TState, TArg, TResult>(this IFuncPublisher<TArg, TResult> publisher, TArg arg, TState state, Action<TState, FuncResult<TResult>> forEach) {
        using var handlerSnapshot = publisher.SnapshotHandlers();
        foreach (var handler in handlerSnapshot.Span) {
            forEach(state, publisher.InvokeHandler(handler, arg));
        }
    }
    
    public static void InvokeForEach<TArg, TResult>(this IFuncPublisher<TArg, TResult> publisher, TArg arg, Action<FuncResult<TResult>, int> forEach) {
        using var handlerSnapshot = publisher.SnapshotHandlers();
        var currentIndex = 0;
        foreach (var handler in handlerSnapshot.Span) {
            forEach(publisher.InvokeHandler(handler, arg), currentIndex);
            currentIndex++;
        }
    }
    
    public static void InvokeForEach<TState, TArg, TResult>(this IFuncPublisher<TArg, TResult> publisher, TArg arg, TState state, Action<TState, FuncResult<TResult>, int> forEach) {
        using var handlerSnapshot = publisher.SnapshotHandlers();
        var currentIndex = 0;
        foreach (var handler in handlerSnapshot.Span) {
            forEach(state, publisher.InvokeHandler(handler, arg), currentIndex);
            currentIndex++;
        }
    }
    
    public static void InvokeForEachValue<TArg, TResult>(this IFuncPublisher<TArg, TResult> publisher, TArg arg, Action<TResult> forEach) {
        using var handlerSnapshot = publisher.SnapshotHandlers();
        foreach (var handler in handlerSnapshot.Span) {
            var result = publisher.InvokeHandler(handler, arg);
            if (result.HasValue) {
                forEach.Invoke(result.Value);
            }
        }
    }
    
    public static void InvokeForEachValue<TState, TArg, TResult>(this IFuncPublisher<TArg, TResult> publisher, TArg arg, TState state, Action<TState, TResult> forEach) {
        using var handlerSnapshot = publisher.SnapshotHandlers();
        foreach (var handler in handlerSnapshot.Span) {
            var result = publisher.InvokeHandler(handler, arg);
            if (result.HasValue) {
                forEach.Invoke(state, result.Value);
            }
        }
    }
    
    public static void InvokeForEachValue<TArg, TResult>(this IFuncPublisher<TArg, TResult> publisher, TArg arg, Action<TResult, int> forEach) {
        using var handlerSnapshot = publisher.SnapshotHandlers();
        var currentIndex = 0;
        foreach (var handler in handlerSnapshot.Span) {
            var result = publisher.InvokeHandler(handler, arg);
            if (result.HasValue) {
                forEach.Invoke(result.Value, currentIndex);
            }
            currentIndex++;
        }
    }
    
    public static void InvokeForEachValue<TState, TArg, TResult>(this IFuncPublisher<TArg, TResult> publisher, TArg arg, TState state, Action<TState, TResult, int> forEach) {
        using var handlerSnapshot = publisher.SnapshotHandlers();
        var currentIndex = 0;
        foreach (var handler in handlerSnapshot.Span) {
            var result = publisher.InvokeHandler(handler, arg);
            if (result.HasValue) {
                forEach.Invoke(state, result.Value, currentIndex);
            }
            currentIndex++;
        }
    }
}