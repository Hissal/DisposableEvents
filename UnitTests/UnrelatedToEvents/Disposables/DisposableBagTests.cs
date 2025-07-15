using DisposableEvents.Disposables;

namespace UnitTests.UnrelatedToEvents.Disposables;

[TestFixture]
public class DisposableBagTests {
    [Test]
    public void Add_Dispose_DisposesAll() {
        var bag = new DisposableBag(2);
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        
        bag.Add(d1);
        bag.Add(d2);
        bag.Dispose();
        
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
        });
    }

    [Test]
    public void Add_AfterDispose_ImmediatelyDisposes() {
        var bag = new DisposableBag(1);
        var d = new TestDisposable();
        
        bag.Dispose();
        bag.Add(d);
        
        Assert.That(d.IsDisposed, Is.True);
    }

    [Test]
    public void Clear_DisposesAll() {
        var bag = new DisposableBag(2);
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        
        bag.Add(d1);
        bag.Add(d2);
        bag.Clear();
        
        Assert.Multiple(() => {
            Assert.That(d1.IsDisposed, Is.True);
            Assert.That(d2.IsDisposed, Is.True);
        });
    }

    [Test]
    public void AddAfterClear_DoesNotDispose() {
        var bag = new DisposableBag(2);
        var d1 = new TestDisposable();
        
        bag.Clear();
        bag.Add(d1);
        
        Assert.That(d1.IsDisposed, Is.False);
    }

    [Test]
    public void Dispose_Twice_DoesNotThrow() {
        var bag = new DisposableBag(1);
        var d = new TestDisposable();
        
        bag.Add(d);
        bag.Dispose();
        
        Assert.DoesNotThrow(() => bag.Dispose());
    }
}