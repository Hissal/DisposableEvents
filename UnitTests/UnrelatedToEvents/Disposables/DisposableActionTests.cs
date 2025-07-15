using DisposableEvents.Disposables;

namespace UnitTests.UnrelatedToEvents.Disposables;

[TestFixture]
public class DisposableActionTests {
    // Tests for DisposableAction class
    
    [Test]
    public void DisposableAction_Invoke_InvokesAction() {
        bool invoked = false;
        var action = new DisposableAction(() => invoked = true, () => { });

        action.Invoke();
        Assert.That(invoked, Is.True);
    }

    [Test]
    public void DisposableAction_Dispose_InvokesOnDispose() {
        bool disposed = false;
        var action = new DisposableAction(() => { }, () => disposed = true);

        action.Dispose();
        Assert.That(disposed, Is.True);
    }

    [Test]
    public void DisposableAction_Add_ChainsInvokeAndDispose() {
        bool invoked1 = false, invoked2 = false;
        bool disposed1 = false, disposed2 = false;
        var a1 = new DisposableAction(() => invoked1 = true, () => disposed1 = true);
        var a2 = new DisposableAction(() => invoked2 = true, () => disposed2 = true);

        a1.Add(a2);
        a1.Invoke();
        a1.Dispose();
        
        Assert.Multiple(() => {
            Assert.That(invoked1, Is.True);
            Assert.That(invoked2, Is.True);
            Assert.That(disposed1, Is.True);
            Assert.That(disposed2, Is.True);
        });
    }
    
    // Tests for DisposableAction<T> class

    [Test]
    public void DisposableActionT_Invoke_InvokesAction() {
        int result = 0;
        var action = new DisposableAction<int>(x => result = x, () => { });

        action.Invoke(42);
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void DisposableActionT_Dispose_InvokesOnDispose() {
        bool disposed = false;
        var action = new DisposableAction<int>(_ => { }, () => disposed = true);

        action.Dispose();
        Assert.That(disposed, Is.True);
    }

    [Test]
    public void DisposableActionT_Add_ChainsInvokeAndDispose() {
        int result1 = 0, result2 = 0;
        bool disposed1 = false, disposed2 = false;
        var a1 = new DisposableAction<int>(x => result1 = x, () => disposed1 = true);
        var a2 = new DisposableAction<int>(x => result2 = x + 1, () => disposed2 = true);

        a1.Add(a2);
        a1.Invoke(10);
        a1.Dispose();

        Assert.Multiple(() => {
            Assert.That(result1, Is.EqualTo(10));
            Assert.That(result2, Is.EqualTo(11));
            Assert.That(disposed1, Is.True);
            Assert.That(disposed2, Is.True);
        });
    }

    // Tests for DisposableActionClosure class

    [Test]
    public void DisposableActionClosure_Invoke_InvokesActionWithClosure() {
        int closure = 5;
        int invokedValue = 0;
        var action = new DisposableActionClosure<int>(closure, c => invokedValue = c * 2, c => { });

        action.Invoke();
        Assert.That(invokedValue, Is.EqualTo(10));
    }

    [Test]
    public void DisposableActionClosure_Dispose_InvokesOnDisposeWithClosure() {
        int closure = 7;
        int disposedValue = 0;
        var action = new DisposableActionClosure<int>(closure, _ => { }, c => disposedValue = c + 1);

        action.Dispose();
        Assert.That(disposedValue, Is.EqualTo(8));
    }

    [Test]
    public void DisposableActionClosure_Add_ChainsInvokeAndDispose() {
        int closure = 2;
        int invoked1 = 0, invoked2 = 0;
        int disposed1 = 0, disposed2 = 0;
        var a1 = new DisposableActionClosure<int>(closure, c => invoked1 = c, c => disposed1 = c + 1);
        var a2 = new DisposableActionClosure<int>(closure, c => invoked2 = c * 2, c => disposed2 = c + 2);

        a1.Add(a2);
        a1.Invoke();
        a1.Dispose();

        Assert.Multiple(() => {
            Assert.That(invoked1, Is.EqualTo(2));
            Assert.That(invoked2, Is.EqualTo(4));
            Assert.That(disposed1, Is.EqualTo(3));
            Assert.That(disposed2, Is.EqualTo(4));
        });
    }

    // Tests for DisposableActionClosure<T> class

    [Test]
    public void DisposableActionClosureT_Invoke_InvokesActionWithClosureAndArg() {
        string closure = "abc";
        string? invokedValue = null;
        var action = new DisposableActionClosure<string, int>(closure, (c, x) => invokedValue = c + x, c => { });

        action.Invoke(7);
        Assert.That(invokedValue, Is.EqualTo("abc7"));
    }

    [Test]
    public void DisposableActionClosureT_Dispose_InvokesOnDisposeWithClosure() {
        string closure = "xyz";
        string? disposedValue = null;
        var action = new DisposableActionClosure<string, int>(closure, (_, _) => { }, c => disposedValue = c + "!");

        action.Dispose();
        Assert.That(disposedValue, Is.EqualTo("xyz!"));
    }

    [Test]
    public void DisposableActionClosureT_Add_ChainsInvokeAndDispose() {
        string closure = "x";
        string? invoked1 = null, invoked2 = null;
        string? disposed1 = null, disposed2 = null;
        var a1 = new DisposableActionClosure<string, int>(closure, (c, x) => invoked1 = c + x, c => disposed1 = c + "!");
        var a2 = new DisposableActionClosure<string, int>(closure, (c, x) => invoked2 = c + (x * 2), c => disposed2 = c + "?");

        a1.Add(a2);
        a1.Invoke(3);
        a1.Dispose();

        Assert.Multiple(() => {
            Assert.That(invoked1, Is.EqualTo("x3"));
            Assert.That(invoked2, Is.EqualTo("x6"));
            Assert.That(disposed1, Is.EqualTo("x!"));
            Assert.That(disposed2, Is.EqualTo("x?"));
        });
    }
    
    // Tests for combining non closure and closure actions
    
    [Test]
    public void DisposableAction_CombinesWithDisposableActionClosure() {
        int closure = 10;
        
        int invokedValue = 0;
        
        bool actionDisposed = false;
        bool actionClosureDisposed = false;

        var action = new DisposableAction(() => invokedValue += 5, () => actionDisposed = true);
        var actionClosure = new DisposableActionClosure<int>(closure, c => invokedValue += c, _ => actionClosureDisposed = true);

        action.Add(actionClosure);
        action.Invoke();
        action.Dispose();

        Assert.Multiple(() => {
            Assert.That(invokedValue, Is.EqualTo(15)); // 10 + 5
            Assert.That(actionDisposed, Is.True);
            Assert.That(actionClosureDisposed, Is.True);
        });
    }
    
    [Test]
    public void DisposableActionT_CombinesWithDisposableActionClosureT() {
        string closure = "test";
        
        string? invokedValue = null;
        
        bool actionDisposed = false;
        bool actionClosureDisposed = false;

        var action = new DisposableAction<int>(x => invokedValue = x.ToString(), () => actionDisposed = true);
        var actionClosure = new DisposableActionClosure<string, int>(closure, (c, x) => invokedValue += c + x, _ => actionClosureDisposed = true);

        action.Add(actionClosure);
        action.Invoke(42);
        action.Dispose();

        Assert.Multiple(() => {
            Assert.That(invokedValue, Is.EqualTo("42test42")); // 42 + "test" + 42
            Assert.That(actionDisposed, Is.True);
            Assert.That(actionClosureDisposed, Is.True);
        });
    }
    
    // Tests for static factory methods

    [Test]
    public void DisposableAction_Create_ReturnsDisposableAction() {
        var a = DisposableAction.Create(() => { }, () => { });
        Assert.That(a, Is.InstanceOf<DisposableAction>());
    }

    [Test]
    public void DisposableActionT_Create_ReturnsDisposableActionT() {
        var a = DisposableAction.Create<int>(x => { }, () => { });
        Assert.That(a, Is.InstanceOf<DisposableAction<int>>());
    }
    
    [Test]
    public void DisposableActionClosure_Create_ReturnsDisposableActionClosure() {
        var a = DisposableAction.Create(5, c => { }, c => { });
        Assert.That(a, Is.InstanceOf<DisposableActionClosure<int>>());
    }
    
    [Test]
    public void DisposableActionClosureT_Create_ReturnsDisposableActionClosureT() {
        var a = DisposableAction.Create<int, int>(5, (c, x) => { }, c => { });
        Assert.That(a, Is.InstanceOf<DisposableActionClosure<int, int>>());
    }
}