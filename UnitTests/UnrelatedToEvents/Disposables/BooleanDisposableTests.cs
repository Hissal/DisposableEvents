using DisposableEvents.Disposables;

namespace UnitTests.UnrelatedToEvents.Disposables;

[TestFixture]
public class BooleanDisposableTests {
    [Test]
    public void IsDisposed_False_Initially() {
        var d = new BooleanDisposable();
        Assert.That(d.IsDisposed, Is.False);
    }

    [Test]
    public void Dispose_SetsIsDisposed_True() {
        var d = new BooleanDisposable();
        d.Dispose();
        Assert.That(d.IsDisposed, Is.True);
    }

    [Test]
    public void Dispose_CalledMultipleTimes_IsDisposedRemainsTrue() {
        var d = new BooleanDisposable();
        d.Dispose();
        d.Dispose();
        Assert.That(d.IsDisposed, Is.True);
    }
}