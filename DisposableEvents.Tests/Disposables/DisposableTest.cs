using DisposableEvents.Disposables;

namespace DisposableEvents.Tests.Disposables;

[TestSubject(typeof(Disposable))]
public class DisposableTest {
    [Fact]
    public void DisposableEmpty_ReturnsSharedInstance() {
        var d1 = Disposable.Empty;
        var d2 = Disposable.Empty;
        Assert.Same(d1, d2);
    }
    
    [Fact]
    public void ActionDisposable_Dispose_InvokesAction() {
        var invoked = false;
        var sut = Disposable.Action(() => invoked = true);
        sut.Dispose();
        Assert.True(invoked);
    }

    [Fact]
    public void ActionDisposableWithState_Dispose_InvokesAction() {
        var invoked = false;
        var sut = Disposable.Action(this, t => invoked = true);
        sut.Dispose();
        Assert.True(invoked);
    }
    
    [Fact]
    public void BooleanDisposable_IsDisposedReflectsState() {
        var sut = Disposable.Boolean();
        Assert.False(sut.IsDisposed);
        sut.Dispose();
        Assert.True(sut.IsDisposed);
    }
}