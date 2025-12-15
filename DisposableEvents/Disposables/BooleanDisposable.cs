namespace DisposableEvents.Disposables;

/// <summary>
/// Represents a disposable that tracks its disposal state using a thread-safe boolean flag.
/// </summary>
public sealed class BooleanDisposable : IDisposable {
    bool isDisposed;
    
    /// <summary>
    /// Gets a value indicating whether this instance has been disposed.
    /// </summary>
    public bool IsDisposed => Volatile.Read(ref isDisposed);
    
    /// <summary>
    /// Sets the disposal state to true.
    /// </summary>
    public void Dispose() {
        Volatile.Write(ref isDisposed, true);
    }
}