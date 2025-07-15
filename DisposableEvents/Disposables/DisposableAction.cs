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
    public static IDisposableAction Create(Action action, Action onDispose) => new DisposableAction(action, onDispose);
    public static IDisposableAction<T> Create<T>(Action<T> action, Action onDispose) => new DisposableAction<T>(action, onDispose);

    public static IDisposableAction Create<TClosure>(TClosure closure, Action<TClosure> action, Action<TClosure> onDispose) => 
        new DisposableActionClosure<TClosure>(closure, action, onDispose);
    public static IDisposableAction<T> Create<TClosure, T>(TClosure closure, Action<TClosure, T> action, Action<TClosure> onDispose) => 
        new DisposableActionClosure<TClosure, T>(closure, action, onDispose);
    
    Action? action;
    Action? onDispose;
    
    bool isDisposed;
    bool IsDisposed => Volatile.Read(ref isDisposed);
    
    public DisposableAction(Action action, Action onDispose) {
        this.action = action;
        this.onDispose = onDispose;
    }
    
    public void Invoke() {
        if (IsDisposed)
            return;
        
        action?.Invoke();
    }

    public void Add(IDisposableAction action) {
        if (IsDisposed)
            return;
        
        this.action += action.Invoke;
        onDispose += action.Dispose;
    }

    public void Dispose() {
        if (IsDisposed)
            return;
        
        Volatile.Write(ref isDisposed, true);
        Volatile.Write(ref action, null);
        Interlocked.Exchange(ref onDispose, null)?.Invoke();
    }
}

public sealed class DisposableAction<T> : IDisposableAction<T> {
    Action<T>? action;
    Action? onDispose;
    
    bool isDisposed;
    bool IsDisposed => Volatile.Read(ref isDisposed);
    
    public DisposableAction(Action<T> action, Action onDispose) {
        this.action = action;
        this.onDispose = onDispose;
    }
    
    public void Invoke(T arg) {
        if (IsDisposed)
            return;
        
        action?.Invoke(arg);
    }

    public void Add(IDisposableAction<T> action) {
        if (IsDisposed)
            return;
        
        this.action += action.Invoke;
        onDispose += action.Dispose;
    }

    public void Dispose() {
        if (IsDisposed)
            return;
        
        Volatile.Write(ref isDisposed, true);
        Volatile.Write(ref action, null);
        Interlocked.Exchange(ref onDispose, null)?.Invoke();
    }
}

public sealed class DisposableActionClosure<TClosure> : IDisposableAction {
    Action<TClosure>? action;
    Action<TClosure>? onDispose;
    
    bool isDisposed;
    bool IsDisposed => Volatile.Read(ref isDisposed);

    readonly TClosure closure;
    
    List<IDisposableAction?>? additionalActions;
    
    public DisposableActionClosure(TClosure closure, Action<TClosure> action, Action<TClosure> onDispose) {
        this.closure = closure;
        this.action = action;
        this.onDispose = onDispose;
    }
    
    public void Invoke() {
        if (IsDisposed)
            return;
        
        action?.Invoke(closure);
        
        if (additionalActions == null)
            return;
        
        foreach (var additionalAction in additionalActions) {
            additionalAction?.Invoke();
        }
    }

    public void Add(IDisposableAction action) {
        if (IsDisposed)
            return;
        
        additionalActions ??= new List<IDisposableAction?>();
        additionalActions.Add(action);
    }

    public void Dispose() {
        if (IsDisposed)
            return;
        
        Volatile.Write(ref isDisposed, true);
        Volatile.Write(ref action, null);
        Interlocked.Exchange(ref onDispose, null)?.Invoke(closure);

        if (additionalActions == null)
            return;
        
        foreach (var additionalAction in additionalActions) {
            additionalAction?.Dispose();
        }
        
        additionalActions.Clear();
        additionalActions = null;
    }
}

public sealed class DisposableActionClosure<TClosure, T> : IDisposableAction<T> {
    Action<TClosure, T>? action;
    Action<TClosure>? onDispose;
    
    bool isDisposed;
    bool IsDisposed => Volatile.Read(ref isDisposed);

    readonly TClosure closure;
    
    List<IDisposableAction<T>?>? additionalActions;
    
    public DisposableActionClosure(TClosure closure, Action<TClosure, T> action, Action<TClosure> onDispose) {
        this.closure = closure;
        this.action = action;
        this.onDispose = onDispose;
    }
    
    public void Invoke(T arg) {
        if (IsDisposed)
            return;
        
        action?.Invoke(closure, arg);
        
        if (additionalActions == null)
            return;
        
        foreach (var additionalAction in additionalActions) {
            additionalAction?.Invoke(arg);
        }
    }

    public void Add(IDisposableAction<T> action) {
        if (IsDisposed)
            return;
        
        additionalActions ??= new List<IDisposableAction<T>?>();
        additionalActions.Add(action);
    }

    public void Dispose() {
        if (IsDisposed)
            return;
        
        Volatile.Write(ref isDisposed, true);
        Volatile.Write(ref action, null);
        Interlocked.Exchange(ref onDispose, null)?.Invoke(closure);
        
        if (additionalActions == null)
            return;
        
        foreach (var additionalAction in additionalActions) {
            additionalAction?.Dispose();
        }
        
        additionalActions.Clear();
        additionalActions = null;
    }
}