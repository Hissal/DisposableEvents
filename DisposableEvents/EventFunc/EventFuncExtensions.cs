namespace DisposableEvents;

public static class EventFuncExtensions {
    public static IDisposable Subscribe<TMessage, TReturn>(this IEventFuncSubscriber<TMessage, TReturn> subscriber,
        Func<TMessage, FuncResult<TReturn>> func, params IEventFilter<TMessage>[] filters) 
    {
        return subscriber.Subscribe(new EventFuncObserver<TMessage, TReturn>(func), filters);
    }
}