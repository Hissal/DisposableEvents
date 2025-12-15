using DisposableEvents.Disposables;

namespace DisposableEvents;

public interface IPipelineEvent<TMessage> : IDisposableEvent<TMessage> {
    IPipelineEvent<TMessage>? Next { get; }
    void SetNext(IPipelineEvent<TMessage> next);
}

public sealed partial class EventPipeline<TMessage> : IDisposableEvent<TMessage> {
    public IPipelineEvent<TMessage> Pipeline { get; }

    DisposableBag disposables;
    
    public bool IsDisposed => Pipeline.IsDisposed;
    public int HandlerCount => Pipeline.HandlerCount;
    
    internal EventPipeline(IPipelineEvent<TMessage> pipeline, DisposableBag disposables = default) {
        Pipeline = pipeline;
        this.disposables = disposables;
    }

    public static EventPipelineBuilder<TMessage> Manual(IPipelineEvent<TMessage> first) {
        return new EventPipelineBuilder<TMessage>(first);
    }

    public static EventPipelineBuilder<TMessage> Manual() {
        return new EventPipelineBuilder<TMessage>();
    }

    public void Publish(TMessage message) => Pipeline.Publish(message);
    public IDisposable Subscribe(IEventHandler<TMessage> handler) => Pipeline.Subscribe(handler);
    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage> filter) => Pipeline.Subscribe(handler, filter);
    public IDisposable Subscribe(IEventHandler<TMessage> handler, IEventFilter<TMessage>[] filters, FilterOrdering ordering) => Pipeline.Subscribe(handler, filters, ordering);
   
    public void Dispose() {
        disposables.Dispose();
        Pipeline.Dispose();
    }

    public EventHandlerSnapshot<TMessage> SnapshotHandlers() => Pipeline.SnapshotHandlers();
    public void ClearSubscriptions() => Pipeline.ClearSubscriptions();
}

public sealed class EventPipelineBuilder<TMessage> {
    readonly IPipelineEvent<TMessage> first;
    public IPipelineEvent<TMessage> Current { get; private set; }

    DisposableBag disposables;
    
    internal EventPipelineBuilder() : this(new LazyInnerEvent<TMessage>()) { }

    internal EventPipelineBuilder(IPipelineEvent<TMessage> first) {
        this.first = first;
        Current = first;
    }
    
    public EventPipelineBuilder<TMessage> Next<TNextEvent>(TNextEvent next) where TNextEvent : IPipelineEvent<TMessage>{
        Current.SetNext(next);
        Current = next;
        return this;
    }
    public EventPipelineBuilder<TMessage> Next(Func<IPipelineEvent<TMessage>> nextFactory) {
        var next = nextFactory();
        Current.SetNext(next);
        Current = next;
        return this;
    }
    
    public EventPipelineBuilder<TMessage> AddDisposable(IDisposable disposable) {
        disposables.Add(disposable);
        return this;
    }
    
    public EventPipeline<TMessage> Build() {
        if (first is LazyInnerEvent<TMessage> lazyInner) {
            return new EventPipeline<TMessage>(lazyInner.Next ?? lazyInner, disposables);
        }
        
        return new EventPipeline<TMessage>(first, disposables);
    }
}