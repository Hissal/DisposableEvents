namespace DisposableEvents;

public static partial class FuncPublisherExtensions {
    public static FuncResult<TReturn>[] PublishToArray<TMessage, TReturn>(
        this IFuncPublisher<TMessage, TReturn> publisher, TMessage message) {
        var handlers = publisher.GetHandlers();

        if (handlers.Length == 0)
            return Array.Empty<FuncResult<TReturn>>();

        var results = new FuncResult<TReturn>[handlers.Length];

        for (var i = 0; i < handlers.Length; i++) {
            results[i] = publisher.PublishTo(handlers[i], message);
        }

        return results;
    }

    public static int PublishToArrayNonAlloc<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher,
        TMessage message, FuncResult<TReturn>[] results) {
        var handlers = publisher.GetHandlers();

        if (results.Length == 0)
            return 0;

        var count = Math.Min(handlers.Length, results.Length);
        for (var i = 0; i < count; i++) {
            results[i] = publisher.PublishTo(handlers[i], message);
        }

        return count;
    }

    public static int PublishToArrayNonAlloc<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher,
        TMessage message, TReturn[] results) {
        var handlers = publisher.GetHandlers();

        if (results.Length == 0)
            return 0;

        var count = Math.Min(handlers.Length, results.Length);
        for (var i = 0; i < count; i++) {
            var funcResult = publisher.PublishTo(handlers[i], message);
            if (funcResult.HasValue) {
                results[i] = funcResult.Value;
            }
        }

        return count;
    }
}