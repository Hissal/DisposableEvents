using DisposableEvents;

namespace UnitTests.EventParts.EventObservers;

[TestFixture]
public class EventObserverTests {
    // Generic Tests
    
    [Test]
    public void OnNext_CallsAction() {
        int received = 0;
        var obs = new EventObserver<int>(x => received = x);
        obs.OnNext(42);
        Assert.That(received, Is.EqualTo(42));
    }

    [Test]
    public void OnError_CallsOnError() {
        Exception captured = null;
        var obs = new EventObserver<int>(_ => { }, ex => captured = ex);
        var ex = new InvalidOperationException("fail");
        obs.OnError(ex);
        Assert.That(captured, Is.EqualTo(ex));
    }

    [Test]
    public void OnCompleted_CallsOnCompleted() {
        bool completed = false;
        var obs = new EventObserver<int>(_ => { }, null, () => completed = true);
        obs.OnCompleted();
        Assert.That(completed, Is.True);
    }

    [Test]
    public void OnError_WithoutHandler_Throws() {
        var obs = new EventObserver<int>(_ => { });
        Assert.Throws<Exception>(() => obs.OnError(new Exception()));
    }

    [Test]
    public void OnCompleted_WithoutHandler_DoesNotThrow() {
        var obs = new EventObserver<int>(_ => { });
        Assert.DoesNotThrow(() => obs.OnCompleted());
    }

    [Test]
    public void NullOnNext_DoesNotThrow() {
        Assert.DoesNotThrow(() => {
            var obs = new EventObserver<int>(null!);
            obs.OnNext(42);
        });
    }
    
    // Empty event observer tests
    
    [Test]
    public void Empty_OnNext_CallsAction() {
        int received = 0;
        var obs = new EmptyEventObserver(() => received = 42);
        obs.OnNext(new EmptyEvent());
        Assert.That(received, Is.EqualTo(42));
    }

    [Test]
    public void Empty_OnError_CallsOnError() {
        Exception captured = null;
        var obs = new EmptyEventObserver(() => { }, ex => captured = ex);
        var ex = new InvalidOperationException("fail");
        obs.OnError(ex);
        Assert.That(captured, Is.EqualTo(ex));
    }

    [Test]
    public void Empty_OnCompleted_CallsOnCompleted() {
        bool completed = false;
        var obs = new EmptyEventObserver(() => { }, null, () => completed = true);
        obs.OnCompleted();
        Assert.That(completed, Is.True);
    }

    [Test]
    public void Empty_OnError_WithoutHandler_Throws() {
        var obs = new EmptyEventObserver(() => { });
        Assert.Throws<Exception>(() => obs.OnError(new Exception()));
    }

    [Test]
    public void Empty_OnCompleted_WithoutHandler_DoesNotThrow() {
        var obs = new EmptyEventObserver(() => { });
        Assert.DoesNotThrow(() => obs.OnCompleted());
    }

    [Test]
    public void Empty_NullOnNext_DoesNotThrow() {
        Assert.DoesNotThrow(() => {
            var obs = new EmptyEventObserver(null!);
            obs.OnNext(new EmptyEvent());
        });
    }
}