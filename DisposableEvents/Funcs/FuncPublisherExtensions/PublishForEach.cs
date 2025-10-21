namespace DisposableEvents;

public static partial class FuncPublisherExtensions {
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