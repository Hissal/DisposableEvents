using DisposableEvents.Disposables;

namespace UnitTests.UnrelatedToEvents.Disposables;

[TestFixture]
public class DisposableCombineTests {
    [Test]
    public void Combine2_DisposesBoth() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var combined = Disposable.Combine(d1, d2);
        combined.Dispose();
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Combine3_DisposesAll() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        var combined = Disposable.Combine(d1, d2, d3);
        combined.Dispose();
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
            Assert.That(d3.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Combine4_DisposesAll() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        var d4 = new TestDisposable();
        var combined = Disposable.Combine(d1, d2, d3, d4);
        combined.Dispose();
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
            Assert.That(d3.IsDisposed, Is.True);
            Assert.That(d4.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Combine5_DisposesAll() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        var d4 = new TestDisposable();
        var d5 = new TestDisposable();
        var combined = Disposable.Combine(d1, d2, d3, d4, d5);
        combined.Dispose();
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
            Assert.That(d3.IsDisposed, Is.True);
            Assert.That(d4.IsDisposed, Is.True);
            Assert.That(d5.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Combine6_DisposesAll() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        var d4 = new TestDisposable();
        var d5 = new TestDisposable();
        var d6 = new TestDisposable();
        var combined = Disposable.Combine(d1, d2, d3, d4, d5, d6);
        combined.Dispose();
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
            Assert.That(d3.IsDisposed, Is.True);
            Assert.That(d4.IsDisposed, Is.True);
            Assert.That(d5.IsDisposed, Is.True);
            Assert.That(d6.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Combine7_DisposesAll() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        var d4 = new TestDisposable();
        var d5 = new TestDisposable();
        var d6 = new TestDisposable();
        var d7 = new TestDisposable();
        var combined = Disposable.Combine(d1, d2, d3, d4, d5, d6, d7);
        combined.Dispose();
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
            Assert.That(d3.IsDisposed, Is.True);
            Assert.That(d4.IsDisposed, Is.True);
            Assert.That(d5.IsDisposed, Is.True);
            Assert.That(d6.IsDisposed, Is.True);
            Assert.That(d7.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Combine8_DisposesAll() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        var d4 = new TestDisposable();
        var d5 = new TestDisposable();
        var d6 = new TestDisposable();
        var d7 = new TestDisposable();
        var d8 = new TestDisposable();
        var combined = Disposable.Combine(d1, d2, d3, d4, d5, d6, d7, d8);
        combined.Dispose();
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
            Assert.That(d3.IsDisposed, Is.True);
            Assert.That(d4.IsDisposed, Is.True);
            Assert.That(d5.IsDisposed, Is.True);
            Assert.That(d6.IsDisposed, Is.True);
            Assert.That(d7.IsDisposed, Is.True);
            Assert.That(d8.IsDisposed, Is.True);
        });
    }

    [Test]
    public void CombineArray_DisposesAll() {
        var disposables = Enumerable.Range(0, 12).Select(_ => new TestDisposable()).ToArray();
        var combined = Disposable.Combine(disposables);
        combined.Dispose();
        foreach (var d in disposables)
            Assert.That(d.IsDisposed, Is.True);
    }
}