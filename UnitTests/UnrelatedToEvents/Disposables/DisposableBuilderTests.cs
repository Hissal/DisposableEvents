using DisposableEvents.Disposables;

namespace UnitTests.UnrelatedToEvents.Disposables;

[TestFixture]
public class DisposableBuilderTests {
    [Test]
    public void Build_CombinesDisposables() {
        var builder = new DisposableBuilder();
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();

        builder.Add(d1);
        builder.Add(d2);
        var combined = builder.Build();
        combined.Dispose();

        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Add_MoreThanEight_HandlesAll() {
        var builder = new DisposableBuilder();
        var disposables = Enumerable.Range(0, 10).Select(_ => new TestDisposable()).ToArray();

        foreach (var d in disposables)
            builder.Add(d);

        var combined = builder.Build();
        combined.Dispose();

        foreach (var d in disposables)
            Assert.That(d.IsDisposed, Is.True);
    }

    [Test]
    public void Dispose_BeforeBuild_DoesNotDisposeAddedDisposables() {
        var builder = new DisposableBuilder();
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();

        builder.Add(d1);
        builder.Add(d2);
        builder.Dispose();

        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.False);
            Assert.That(d2.IsDisposed, Is.False);
        });
    }

    [Test]
    public void Add_AfterDispose_ImmediatelyDisposes() {
        var builder = new DisposableBuilder();
        var d = new TestDisposable();

        builder.Dispose();
        builder.Add(d);

        Assert.That(d.IsDisposed, Is.True);
    }

    [Test]
    public void Build_Empty_ReturnsDisposableEmpty() {
        var builder = new DisposableBuilder();
        var result = builder.Build();

        Assert.That(result, Is.Not.Null);
        Assert.DoesNotThrow(() => result.Dispose());
    }

    [Test]
    public void Build_SingleDisposable_ReturnsIt() {
        var builder = new DisposableBuilder();
        var d = new TestDisposable();

        builder.Add(d);
        var result = builder.Build();
        result.Dispose();
        Assert.Multiple(() => {
            Assert.That(result, Is.SameAs(d));
            Assert.That(d.IsDisposed, Is.True);
        });
    }
}