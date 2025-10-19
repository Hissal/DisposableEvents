namespace DisposableEvents;

public static class FuncPublisherExtensions {
    public static FuncResult<TReturn>[] PublishToArray<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message) {
        var handlers = publisher.GetHandlers();
        
        if (handlers.Length == 0)
            return Array.Empty<FuncResult<TReturn>>();
        
        var results = new FuncResult<TReturn>[handlers.Length];

        for (var i = 0; i < handlers.Length; i++) {
            results[i] = publisher.PublishTo(handlers[i], message);
        }
        
        return results;
    }
    
    public static int PublishToArrayNonAlloc<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, FuncResult<TReturn>[] results) {
        var handlers = publisher.GetHandlers();

        if (results.Length == 0)
            return 0;

        var count = Math.Min(handlers.Length, results.Length);
        for (var i = 0; i < count; i++) {
            results[i] = publisher.PublishTo(handlers[i], message);
        }
        return count;
    }
    
    public static int PublishToArrayNonAlloc<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, TReturn[] results) {
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
    
    public static void PublishForEach<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, Action<FuncResult<TReturn>> forEach) {
        foreach (var handler in publisher.GetHandlers()) {
            forEach(publisher.PublishTo(handler, message));
        }
    }
    
    public static void PublishForEach<TState, TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, TState state, Action<TState, FuncResult<TReturn>> forEach) {
        foreach (var handler in publisher.GetHandlers()) {
            forEach(state, publisher.PublishTo(handler, message));
        }
    }
    
    public static void PublishForEach<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, Action<FuncResult<TReturn>, int> forEach) {
        var currentIndex = 0;
        foreach (var handler in publisher.GetHandlers()) {
            forEach(publisher.PublishTo(handler, message), currentIndex);
            currentIndex++;
        }
    }
    
    public static void PublishForEach<TState, TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, TState state, Action<TState, FuncResult<TReturn>, int> forEach) {
        var currentIndex = 0;
        foreach (var handler in publisher.GetHandlers()) {
            forEach(state, publisher.PublishTo(handler, message), currentIndex);
            currentIndex++;
        }
    }
    
    public static void PublishForEachValue<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, Action<TReturn> forEach) {
        foreach (var handler in publisher.GetHandlers()) {
            var result = publisher.PublishTo(handler, message);
            if (result.HasValue) {
                forEach.Invoke(result.Value);
            }
        }
    }
    
    public static void PublishForEachValue<TState, TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, TState state, Action<TState, TReturn> forEach) {
        foreach (var handler in publisher.GetHandlers()) {
            var result = publisher.PublishTo(handler, message);
            if (result.HasValue) {
                forEach.Invoke(state, result.Value);
            }
        }
    }
    
    public static void PublishForEachValue<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, Action<TReturn, int> forEach) {
        var currentIndex = 0;
        foreach (var handler in publisher.GetHandlers()) {
            var result = publisher.PublishTo(handler, message);
            if (result.HasValue) {
                forEach.Invoke(result.Value, currentIndex);
            }
            currentIndex++;
        }
    }
    
    public static void PublishForEachValue<TState, TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, TState state, Action<TState, TReturn, int> forEach) {
        var currentIndex = 0;
        foreach (var handler in publisher.GetHandlers()) {
            var result = publisher.PublishTo(handler, message);
            if (result.HasValue) {
                forEach.Invoke(state, result.Value, currentIndex);
            }
            currentIndex++;
        }
    }
}