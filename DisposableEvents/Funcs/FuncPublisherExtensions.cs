namespace DisposableEvents;

public static class FuncPublisherExtensions {
    public static void PublishForEach<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, Action<TReturn> forEach) {
        foreach (var handler in publisher.GetHandlers()) {
            var result = publisher.PublishTo(handler, message);
            if (result.HasValue) {
                forEach.Invoke(result.Value);
            }
        }
    }
    
    public static void PublishForEach<TState, TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, TState state, Action<TState, TReturn> forEach) {
        foreach (var handler in publisher.GetHandlers()) {
            var result = publisher.PublishTo(handler, message);
            if (result.HasValue) {
                forEach.Invoke(state, result.Value);
            }
        }
    }
    
    public static IEnumerable<FuncResult<TReturn>> PublishAsEnumerable<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message) {
        var handlers = publisher.GetHandlers();
        foreach (var handler in handlers) {
            yield return publisher.PublishTo(handler, message);
        }
    }
    
    public static FuncResult<TReturn>[] PublishToArray<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message) {
        var handlers = publisher.GetHandlers();
        
        if (handlers.Length == 0)
            return Array.Empty<FuncResult<TReturn>>();
        
        var results = new FuncResult<TReturn>[handlers.Length];
        var current = 0;
        foreach (var handler in handlers) {
            results[current++] = publisher.PublishTo(handler, message);
        }
        
        return results;
    }
    
    public static int PublishToArrayNonAlloc<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, FuncResult<TReturn>[] results) {
        var handlers = publisher.GetHandlers();

        if (results.Length == 0)
            return 0;
        
        var current = 0;
        foreach (var handler in handlers) {
            results[current++] = publisher.PublishTo(handler, message);
            
            if (current >= results.Length)
                break;
        }
        
        return current;
    }
    
    public static int PublishToArrayNonAlloc<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message, TReturn?[] results) {
        var handlers = publisher.GetHandlers();

        if (results.Length == 0)
            return 0;
        
        var current = 0;
        foreach (var handler in handlers) {
            results[current++] = publisher.PublishTo(handler, message);
            
            if (current >= results.Length)
                break;
        }
        
        return current;
    }
}