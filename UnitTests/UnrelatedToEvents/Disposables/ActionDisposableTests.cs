using DisposableEvents.Disposables;

namespace UnitTests.UnrelatedToEvents.Disposables;

[TestFixture]
public class ActionDisposableTests {
    [Test]
    public void ActionDisposable_InvokesOnDispose() {
        bool disposed = false;
        var d = Disposable.Action(() => disposed = true);
        d.Dispose();
        Assert.That(disposed, Is.True);
    }

    [Test]
    public void ActionDisposable_OnlyInvokesOnce() {
        int count = 0;
        var d = Disposable.Action(() => count++);
        d.Dispose();
        d.Dispose();
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void ActionDisposableT_InvokesOnDisposeWithClosure() {
        string closure = "abc";
        string? disposedValue = null;
        var d = Disposable.Action(closure, c => disposedValue = c + "!");
        d.Dispose();
        Assert.That(disposedValue, Is.EqualTo("abc!"));
    }

    [Test]
    public void ActionDisposableT_OnlyInvokesOnce() {
        int closure = 42;
        int count = 0;
        var d = Disposable.Action(closure, c => count += c);
        d.Dispose();
        d.Dispose();
        Assert.That(count, Is.EqualTo(42));
    }
}