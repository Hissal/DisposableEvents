namespace DisposableEvents;

public static partial class FuncPublisherExtensions {
    public static FuncResult<TResult>[] InvokeToArray<TArg, TResult>(
        this IFuncPublisher<TArg, TResult> publisher, TArg arg) {
        var handlers = publisher.GetHandlers();

        if (handlers.Length == 0)
            return Array.Empty<FuncResult<TResult>>();

        var results = new FuncResult<TResult>[handlers.Length];

        for (var i = 0; i < handlers.Length; i++) {
            results[i] = publisher.InvokeHandler(handlers[i], arg);
        }

        return results;
    }

    public static int InvokeToArrayNonAlloc<TArg, TResult>(this IFuncPublisher<TArg, TResult> publisher,
        TArg arg, FuncResult<TResult>[] results) {
        var handlers = publisher.GetHandlers();

        if (results.Length == 0)
            return 0;

        var count = Math.Min(handlers.Length, results.Length);
        for (var i = 0; i < count; i++) {
            results[i] = publisher.InvokeHandler(handlers[i], arg);
        }

        return count;
    }

    public static int InvokeToArrayNonAlloc<TArg, TResult>(this IFuncPublisher<TArg, TResult> publisher,
        TArg arg, TResult[] results) {
        var handlers = publisher.GetHandlers();

        if (results.Length == 0)
            return 0;

        var count = Math.Min(handlers.Length, results.Length);
        for (var i = 0; i < count; i++) {
            var funcResult = publisher.InvokeHandler(handlers[i], arg);
            if (funcResult.HasValue) {
                results[i] = funcResult.Value;
            }
        }

        return count;
    }
}