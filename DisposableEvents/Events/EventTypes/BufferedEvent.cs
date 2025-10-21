﻿using DisposableEvents.Internal;

namespace DisposableEvents;

public sealed class BufferedEvent<TMessage> : IPipelineEvent<TMessage> {
    readonly LazyInnerEvent<TMessage> core;

    public bool IsDisposed => core.IsDisposed;

    Optional<TMessage> previousMessage;
    
    public BufferedEvent() : this(GlobalConfig.InitialSubscriberCapacity) { }

    public BufferedEvent(int expectedSubscriberCount) {
        core = new LazyInnerEvent<TMessage>(expectedSubscriberCount);
        previousMessage = Optional<TMessage>.Null();
    }

    public void Publish(TMessage message) {
        previousMessage = Optional<TMessage>.From(message);
        core.Publish(message);
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler) {
        if (!core.IsDisposed && previousMessage.TryGetValue(out var message))
            handler.Handle(message);

        return core.Subscribe(handler);
    }

    public void ClearSubscriptions() => core.ClearSubscriptions();
    public void ClearBufferedMessage() => previousMessage = Optional<TMessage>.Null();
    public void Dispose() => core.Dispose();

    IPipelineEvent<TMessage>? IPipelineEvent<TMessage>.Next => core.Next;
    void IPipelineEvent<TMessage>.SetNext(IPipelineEvent<TMessage> next) => core.SetNext(next);
}

public static class EventPipelineBufferedEventExtensions {
    public static EventPipelineBuilder<TMessage> BufferResponse<TMessage>(this EventPipelineBuilder<TMessage> builder) {
        return builder.Next(new BufferedEvent<TMessage>());
    }
}