namespace DisposableEvents.Disposables;

public interface IDisposableAction : IDisposable {
    void Invoke();
    void Add(IDisposableAction action);
    
    public static IDisposableAction operator +(IDisposableAction lhs, IDisposableAction rhs) {
        lhs.Add(rhs);
        return lhs;
    }
}

public interface IDisposableAction<T> : IDisposable {
    void Invoke(T arg);
    void Add(IDisposableAction<T> action);
    
    public static IDisposableAction<T> operator +(IDisposableAction<T> lhs, IDisposableAction<T> rhs) {
        lhs.Add(rhs);
        return lhs;
    }
}

public sealed class DisposableAction : IDisposableAction {
    Action action;
    Action onDispose;
    
    bool isDisposed;
    
    public DisposableAction(Action action, Action onDispose) {
        this.action = action;
        this.onDispose = onDispose;
    }
    
    public void Invoke() {
        if (isDisposed)
            return;
        
        action.Invoke();
    }

    public void Add(IDisposableAction action) {
        this.action += action.Invoke;
        onDispose += action.Dispose;
    }

    public void Dispose() {
        if (isDisposed)
            return;
        
        onDispose.Invoke();
        
        isDisposed = true;
        action = null!;
        onDispose = null!;
    }
}

public sealed class DisposableAction<T> : IDisposableAction<T> {
    Action<T> action;
    Action onDispose;
    
    bool isDisposed;
    
    public DisposableAction(Action<T> action, Action onDispose) {
        this.action = action;
        this.onDispose = onDispose;
    }
    
    public void Invoke(T arg) {
        if (isDisposed)
            return;
        
        action.Invoke(arg);
    }

    public void Add(IDisposableAction<T> action) {
        this.action += action.Invoke;
        onDispose += action.Dispose;
    }

    public void Dispose() {
        if (isDisposed)
            return;
        
        onDispose.Invoke();
        
        isDisposed = true;
        action = null!;
        onDispose = null!;
    }
}

public sealed class DisposableActionClosure<TClosure> : IDisposableAction {
    Action<TClosure> action;
    Action<TClosure> onDispose;
    
    bool isDisposed;

    readonly TClosure closure;
    
    List<IDisposableAction>? additionalActions;
    
    public DisposableActionClosure(TClosure closure, Action<TClosure> action, Action<TClosure> onDispose) {
        this.closure = closure;
        this.action = action;
        this.onDispose = onDispose;
    }
    
    public void Invoke() {
        if (isDisposed)
            return;
        
        action.Invoke(closure);
        
        if (additionalActions == null)
            return;
        
        foreach (var additionalAction in additionalActions) {
            additionalAction.Invoke();
        }
    }

    public void Add(IDisposableAction action) {
        additionalActions ??= new List<IDisposableAction>();
        additionalActions.Add(action);
    }

    public void Dispose() {
        if (isDisposed)
            return;
        
        onDispose.Invoke(closure);
        
        isDisposed = true;
        action = null!;
        onDispose = null!;

        if (additionalActions == null)
            return;
        
        foreach (var additionalAction in additionalActions) {
            additionalAction.Dispose();
        }
        
        additionalActions.Clear();
        additionalActions = null;
    }
}

public sealed class DisposableActionClosure<TClosure, T> : IDisposableAction<T> {
    Action<TClosure, T> action;
    Action<TClosure> onDispose;
    
    bool isDisposed;

    readonly TClosure closure;
    
    List<IDisposableAction<T>>? additionalActions;
    
    public DisposableActionClosure(TClosure closure, Action<TClosure, T> action, Action<TClosure> onDispose) {
        this.closure = closure;
        this.action = action;
        this.onDispose = onDispose;
    }
    
    public void Invoke(T arg) {
        if (isDisposed)
            return;
        
        action.Invoke(closure, arg);
        
        if (additionalActions == null)
            return;
        
        foreach (var additionalAction in additionalActions) {
            additionalAction.Invoke(arg);
        }
    }

    public void Add(IDisposableAction<T> action) {
        additionalActions ??= new List<IDisposableAction<T>>();
        additionalActions.Add(action);
    }

    public void Dispose() {
        if (isDisposed)
            return;
        
        onDispose.Invoke(closure);
        
        isDisposed = true;
        action = null!;
        onDispose = null!;
        
        if (additionalActions == null)
            return;
        
        foreach (var additionalAction in additionalActions) {
            additionalAction.Dispose();
        }
    }
}