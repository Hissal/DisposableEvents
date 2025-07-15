namespace UnitTests.UnrelatedToEvents.Disposables;

public class TestDisposable : IDisposable {
    public bool IsDisposed { get; private set; }
    public void Dispose() => IsDisposed = true;
}