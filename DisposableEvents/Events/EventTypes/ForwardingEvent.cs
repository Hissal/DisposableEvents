using DisposableEvents.Disposables;
using DisposableEvents.Internal;

namespace DisposableEvents;

[Flags]
public enum ForwardFlags {
    None = 0,
    Publish = 1,
    Subscribe = 2,
    Dispose = 4,
    ClearSubscriptions = 8,
    IncludeSelf = 16,
    
    PubSub = Publish | Subscribe,
    AllWithSelf = Publish | Subscribe | Dispose | ClearSubscriptions | IncludeSelf,
    AllWithoutSelf = Publish | Subscribe | Dispose | ClearSubscriptions,
}

public enum ForwardTiming {
    AfterSelf,
    BeforeSelf,
}

public sealed class ForwardingEvent<TMessage> : AbstractSubscriber<TMessage>, IPipelineEvent<TMessage> {
    readonly LazyInnerEvent<TMessage>? core;
    readonly ArrayOrOne<IDisposableEvent<TMessage>> forwardTargets;

    readonly ForwardFlags forwardFlags;
    readonly ForwardTiming forwardTiming;
    
    int disposed;
    
    public bool IsDisposed => Volatile.Read(ref disposed) != 0 || (core?.IsDisposed ?? false);
    public int HandlerCount => core?.HandlerCount ?? 0;

    public ForwardingEvent(params IDisposableEvent<TMessage>[] forwardTargets)
        : this(forwardTargets, ForwardTiming.AfterSelf) { }
    
    public ForwardingEvent(IDisposableEvent<TMessage>[] forwardTargets, ForwardFlags forwardFlags) 
        : this(forwardTargets, ForwardTiming.AfterSelf, forwardFlags) { }
    
    public ForwardingEvent(IDisposableEvent<TMessage>[] forwardTargets, ForwardTiming forwardTiming = ForwardTiming.AfterSelf, ForwardFlags forwardFlags = ForwardFlags.AllWithoutSelf) {
        this.forwardTargets = forwardTargets;
        this.forwardFlags = forwardFlags;
        this.forwardTiming = forwardTiming;
        
        if (forwardFlags.HasFlag(ForwardFlags.IncludeSelf)) {
            core = new LazyInnerEvent<TMessage>();
        }
    }
    public ForwardingEvent(IDisposableEvent<TMessage> forwardTarget, ForwardTiming forwardTiming = ForwardTiming.AfterSelf, ForwardFlags forwardFlags = ForwardFlags.AllWithSelf) {
        this.forwardTargets = new ArrayOrOne<IDisposableEvent<TMessage>>(forwardTarget);
        this.forwardFlags = forwardFlags;
        this.forwardTiming = forwardTiming;
        
        if (forwardFlags.HasFlag(ForwardFlags.IncludeSelf)) {
            core = new LazyInnerEvent<TMessage>();
        }
    }
    
    public void Publish(TMessage message) {
        if (IsDisposed)
            return;
        
        if (forwardTiming is ForwardTiming.BeforeSelf && forwardFlags.HasFlag(ForwardFlags.Publish)) {
            foreach (var target in forwardTargets) {
                target.Publish(message);
            }
        }
        
        if (forwardFlags.HasFlag(ForwardFlags.IncludeSelf)) {
            core?.Publish(message);
        }
        
        if (forwardTiming is ForwardTiming.AfterSelf && forwardFlags.HasFlag(ForwardFlags.Publish)) {
            foreach (var target in forwardTargets) {
                target.Publish(message);
            }
        }
    }


    public override IDisposable Subscribe(IEventHandler<TMessage> handler) {
        if (IsDisposed) 
            return Disposable.Empty;

        var db = Disposable.CreateBuilder();
        
        if (forwardTiming is ForwardTiming.BeforeSelf && forwardFlags.HasFlag(ForwardFlags.Subscribe)) {
            foreach (var target in forwardTargets) {
                target.Subscribe(handler).AddTo(ref db);
            }
        }
        
        if (forwardFlags.HasFlag(ForwardFlags.IncludeSelf)) {
            core?.Subscribe(handler).AddTo(ref db);
        }
        
        if (forwardTiming is ForwardTiming.AfterSelf && forwardFlags.HasFlag(ForwardFlags.Subscribe)) {
            foreach (var target in forwardTargets) {
                target.Subscribe(handler).AddTo(ref db);
            }
        }

        return db.Build();
    }

    public void Dispose() {
        if (IsDisposed)
            return;
        
        if (Interlocked.Exchange(ref disposed, 1) != 0)
            return;
        
        if (forwardFlags.HasFlag(ForwardFlags.Dispose)) {
            foreach (var target in forwardTargets) {
                target.Dispose();
            }
        }
        
        if (forwardFlags.HasFlag(ForwardFlags.IncludeSelf)) {
            core?.Dispose();
        }
    }

    public void ClearHandlers() {
        if (IsDisposed)
            return;
        
        if (forwardFlags.HasFlag(ForwardFlags.ClearSubscriptions)) {
            foreach (var target in forwardTargets) {
                target.ClearHandlers();
            }
        }
        
        if (forwardFlags.HasFlag(ForwardFlags.IncludeSelf)) {
            core?.ClearHandlers();
        }
    }
    
    public EventHandlerSnapshot<TMessage> SnapshotHandlers() {
        return core != null 
            ? core.SnapshotHandlers() 
            : EventHandlerSnapshot<TMessage>.Empty;
    }

    IPipelineEvent<TMessage>? IPipelineEvent<TMessage>.Next => core?.Next;
    void IPipelineEvent<TMessage>.SetNext(IPipelineEvent<TMessage> next) => core?.SetNext(next);
}

public class ForwardingEventHandler<TMessage> : IEventHandler<TMessage> {
    readonly ArrayOrOne<IEventPublisher<TMessage>> forwardTargets;
    
    public ForwardingEventHandler(IEventPublisher<TMessage> forwardTarget) {
        forwardTargets = new ArrayOrOne<IEventPublisher<TMessage>>(forwardTarget);
    }
    public ForwardingEventHandler(params IEventPublisher<TMessage>[] forwardTargets) {
        this.forwardTargets = forwardTargets;
    }
    
    public void Handle(TMessage message) {
        foreach (var target in forwardTargets) {
            target.Publish(message);
        }
    }
}

public static class ForwardingSubscriptionExtensions {
    public static IDisposable ForwardTo<TMessage>(this IEventSubscriber<TMessage> source, IEventPublisher<TMessage> publisher) {
        return source.Subscribe(new ForwardingEventHandler<TMessage>(publisher));
    }
    public static IDisposable ForwardTo<TMessage>(this IEventSubscriber<TMessage> source, params IEventPublisher<TMessage>[] publishers) {
        return source.Subscribe(new ForwardingEventHandler<TMessage>(publishers));
    }
}

public static class EventPipelineForwardingExtensions {
    public static EventPipelineBuilder<TMessage> ForwardTo<TMessage>(this EventPipelineBuilder<TMessage> builder, params IEventPublisher<TMessage>[] forwardTargets) {
        var sub = builder.Current.Subscribe(new ForwardingEventHandler<TMessage>(forwardTargets));
        return builder.AddDisposable(sub);
    }
    public static EventPipelineBuilder<TMessage> ForwardTo<TMessage>(this EventPipelineBuilder<TMessage> builder, IEventPublisher<TMessage> forwardTarget) {
        var sub = builder.Current.Subscribe(new ForwardingEventHandler<TMessage>(forwardTarget));
        return builder.AddDisposable(sub);
    }
    
    public static EventPipelineBuilder<TMessage> ForwardEvent<TMessage>(
        this EventPipelineBuilder<TMessage> builder, 
        IDisposableEvent<TMessage>[] forwardTargets,
        ForwardTiming forwardTiming = ForwardTiming.AfterSelf,
        ForwardFlags forwardFlags = ForwardFlags.AllWithSelf)
    {
        return builder.Next(new ForwardingEvent<TMessage>(forwardTargets));
    }
    public static EventPipelineBuilder<TMessage> ForwardEvent<TMessage>(
        this EventPipelineBuilder<TMessage> builder, 
        IDisposableEvent<TMessage> forwardTarget, 
        ForwardTiming forwardTiming = ForwardTiming.AfterSelf,
        ForwardFlags forwardFlags = ForwardFlags.AllWithSelf) 
    {
        return builder.Next(new ForwardingEvent<TMessage>(forwardTarget, forwardTiming, forwardFlags));
    }
    public static EventPipelineBuilder<TMessage> ForwardEvent<TMessage>(
        this EventPipelineBuilder<TMessage> builder, 
        IDisposableEvent<TMessage> forwardTarget1, 
        IDisposableEvent<TMessage> forwardTarget2, 
        ForwardTiming forwardTiming = ForwardTiming.AfterSelf,
        ForwardFlags forwardFlags = ForwardFlags.AllWithSelf) 
    {
        return builder.Next(new ForwardingEvent<TMessage>(new[] { forwardTarget1, forwardTarget2 }, forwardTiming, forwardFlags));
    }
    
    public static EventPipelineBuilder<TMessage> ForwardEvent<TMessage>(
        this EventPipelineBuilder<TMessage> builder, 
        IDisposableEvent<TMessage> forwardTarget1, 
        IDisposableEvent<TMessage> forwardTarget2,
        IDisposableEvent<TMessage> forwardTarget3,
        ForwardTiming forwardTiming = ForwardTiming.AfterSelf,
        ForwardFlags forwardFlags = ForwardFlags.AllWithSelf) 
    {
        return builder.Next(new ForwardingEvent<TMessage>(new[] { forwardTarget1, forwardTarget2, forwardTarget3 }, forwardTiming, forwardFlags));
    }
}