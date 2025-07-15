using DisposableEvents.Disposables;

namespace UnitTests.UnrelatedToEvents.Disposables;

[TestFixture]
public class DisposableDisposeTests {
    [Test]
    public void Dispose2_DisposesBoth() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        Disposable.Dispose(d1, d2);
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Dispose3_DisposesAll() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        Disposable.Dispose(d1, d2, d3);
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
            Assert.That(d3.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Dispose4_DisposesAll() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        var d4 = new TestDisposable();
        Disposable.Dispose(d1, d2, d3, d4);
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
            Assert.That(d3.IsDisposed, Is.True);
            Assert.That(d4.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Dispose5_DisposesAll() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        var d4 = new TestDisposable();
        var d5 = new TestDisposable();
        Disposable.Dispose(d1, d2, d3, d4, d5);
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
            Assert.That(d3.IsDisposed, Is.True);
            Assert.That(d4.IsDisposed, Is.True);
            Assert.That(d5.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Dispose6_DisposesAll() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        var d4 = new TestDisposable();
        var d5 = new TestDisposable();
        var d6 = new TestDisposable();
        Disposable.Dispose(d1, d2, d3, d4, d5, d6);
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
    public void Dispose7_DisposesAll() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        var d4 = new TestDisposable();
        var d5 = new TestDisposable();
        var d6 = new TestDisposable();
        var d7 = new TestDisposable();
        Disposable.Dispose(d1, d2, d3, d4, d5, d6, d7);
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
    public void Dispose8_DisposesAll() {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        var d4 = new TestDisposable();
        var d5 = new TestDisposable();
        var d6 = new TestDisposable();
        var d7 = new TestDisposable();
        var d8 = new TestDisposable();
        Disposable.Dispose(d1, d2, d3, d4, d5, d6, d7, d8);
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
    public void DisposeArray_DisposesAll() {
        var disposables = Enumerable.Range(0, 10).Select(_ => new TestDisposable()).ToArray();
        Disposable.Dispose(disposables);
        foreach (var d in disposables)
            Assert.That(d.IsDisposed, Is.True);
    }
}