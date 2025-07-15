using DisposableEvents.Disposables;
using DisposableEvents.Internal;

namespace DisposableEvents;

public sealed class EventFuncCore<TMessage, TReturn> : IDisposable {
    public List<IEventFuncObserver<TMessage, TReturn>> Observers { get; }
    public bool IsDisposed { get; private set; }
    
    public IEventFuncObserver<TMessage, TReturn>[] CopyObservers() => Observers.ToArray();
    
    public EventFuncCore(int expectedSubscriberCount = 2) {
        Observers = new List<IEventFuncObserver<TMessage, TReturn>>(expectedSubscriberCount);
    }
    
    public FuncResult<TReturn> Publish(TMessage message, FuncResultBehavior behavior) {
        if (Observers.Count == 0)
            return default;
        
        FuncResult<TReturn> result = default;
        
        foreach (var observer in Observers) {
            FuncResult<TReturn> resultFromObserver;
            
            try {
                resultFromObserver = observer.OnNext(message);
            }
            catch (Exception e) {
                observer.OnError(e);
                continue;
            }
            
            switch (behavior) {
                case FuncResultBehavior.ReturnLastSuccess:
                    if (resultFromObserver.IsSuccess)
                        result = resultFromObserver;
                    break;
                case FuncResultBehavior.ReturnFirstSuccess:
                    if (resultFromObserver.IsSuccess && !result.IsSuccess) {
                        result = resultFromObserver;
                    }
                    break;
                case FuncResultBehavior.ReturnFirstSuccessAndStop:
                    if (resultFromObserver.IsSuccess) {
                        return resultFromObserver;
                    }
                    break;
                case FuncResultBehavior.ReturnLast:
                    result = resultFromObserver;
                    break;
                case FuncResultBehavior.ReturnFirst:
                    if (!result.IsSuccess) {
                        result = resultFromObserver;
                    }
                    break;
                case FuncResultBehavior.ReturnFirstAndStop:
                    return resultFromObserver;
                default:
                    throw new ArgumentOutOfRangeException(nameof(behavior), behavior, null);
            }
        }
        
        return result;
    }

    public IEnumerable<FuncResult<TReturn>> PublishEnumerator(TMessage message, FuncResultEnumerationBehavior behavior) {
        if (Observers.Count == 0)
            yield break;
        
        foreach (var observer in Observers) {
            FuncResult<TReturn> resultFromObserver;
            
            try {
                resultFromObserver = observer.OnNext(message);
            }
            catch (Exception e) {
                observer.OnError(e);
                continue;
            }
            
            if (behavior is FuncResultEnumerationBehavior.ReturnAll || resultFromObserver.IsSuccess) {
                yield return resultFromObserver;
            }
        }
    }
    
    public IDisposable Subscribe(IEventFuncObserver<TMessage, TReturn> observer) {
        ThrowHelper.ThrowIfNull(observer);
        
        if (IsDisposed) {
            observer.OnCompleted();
            return Disposable.Empty;
        }
        
        Observers.Add(observer);
        // TODO: make subscription class to skip action instance allocation
        return Disposable.Action((core: this, observer: observer), closure => closure.core.Observers.Remove(closure.observer));
    }
    
    ~EventFuncCore() {
        Dispose();
    }
    
    public void Dispose() {
        if (IsDisposed)
            return;

        IsDisposed = true;
        
        foreach (var observer in Observers) {
            try {
                observer.OnCompleted();
            }
            catch (Exception e) {
                observer.OnError(e);
            }
        }
        
        Observers.Clear();
        GC.SuppressFinalize(this);
    }
}