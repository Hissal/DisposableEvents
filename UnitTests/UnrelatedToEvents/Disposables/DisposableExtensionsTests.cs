using DisposableEvents.Disposables;

namespace UnitTests.UnrelatedToEvents.Disposables;

[TestFixture]
public class DisposableExtensionsTests {
    [Test]
    public void AddTo_AddsDisposableToBag() {
        var bag = new DisposableBag(1);
        var d = new TestDisposable();
        
        d.AddTo(ref bag);
        bag.Dispose();
        
        Assert.That(d.IsDisposed, Is.True);
    }

    [Test]
    public void AddTo_AddsMultipleDisposablesToBag() {
        var bag = new DisposableBag(2);
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        
        d1.AddTo(ref bag);
        d2.AddTo(ref bag);
        bag.Dispose();
        
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
        });
    }

    [Test]
    public void AddTo_AddsDisposableToBuilder() {
        var builder = new DisposableBuilder();
        var d = new TestDisposable();

        d.AddTo(ref builder);
        var combined = builder.Build();
        combined.Dispose();

        Assert.That(d.IsDisposed, Is.True);
    }

    [Test]
    public void AddTo_AddsMultipleDisposablesToBuilder() {
        var builder = new DisposableBuilder();
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();

        d1.AddTo(ref builder);
        d2.AddTo(ref builder);
        var combined = builder.Build();
        combined.Dispose();

        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
        });
    }

    [Test]
    public void AddTo_AddsDisposableToCollection() {
        var collection = new List<IDisposable>();
        var d = new TestDisposable();

        d.AddTo(collection);
        Assert.That(collection.Contains(d), Is.True);
    }
}