namespace DisposableEvents;

public static partial class FuncPublisherExtensions {
    public static FuncResult<TReturn>[] InvokeToArray<TArg, TReturn>(
        this IFuncPublisher<TArg, TReturn> publisher, TArg arg) {
        var handlers = publisher.GetHandlers();

        if (handlers.Length == 0)
            return Array.Empty<FuncResult<TReturn>>();

        var results = new FuncResult<TReturn>[handlers.Length];

        for (var i = 0; i < handlers.Length; i++) {
            results[i] = publisher.InvokeHandler(handlers[i], arg);
        }

        return results;
    }

    public static int InvokeToArrayNonAlloc<TArg, TReturn>(this IFuncPublisher<TArg, TReturn> publisher,
        TArg arg, FuncResult<TReturn>[] results) {
        var handlers = publisher.GetHandlers();

        if (results.Length == 0)
            return 0;

        var count = Math.Min(handlers.Length, results.Length);
        for (var i = 0; i < count; i++) {
            results[i] = publisher.InvokeHandler(handlers[i], arg);
        }

        return count;
    }

    public static int InvokeToArrayNonAlloc<TArg, TReturn>(this IFuncPublisher<TArg, TReturn> publisher,
        TArg arg, TReturn[] results) {
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