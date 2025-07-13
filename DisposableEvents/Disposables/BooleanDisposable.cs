namespace DisposableEvents.Disposables;

public sealed class BooleanDisposable : IDisposable {
    bool isDisposed;
    public bool IsDisposed => Volatile.Read(ref isDisposed);
    public void Dispose() {
        Volatile.Write(ref isDisposed, true);
    }
}