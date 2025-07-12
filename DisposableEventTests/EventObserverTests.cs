using DisposableEvents;

namespace DisposableEventTests;

[TestFixture]
public class EventObserverTests {
    [Test]
    public void EventReceiverT_OnNext_CallsAction() {
        int received = 0;
        var receiver = new EventObserver<int>(x => received = x);
        receiver.OnNext(42);
        Assert.That(received, Is.EqualTo(42));
    }

    [Test]
    public void EventReceiverT_OnError_CallsOnError() {
        Exception captured = null;
        var receiver = new EventObserver<int>(_ => { }, ex => captured = ex);
        var ex = new InvalidOperationException("fail");
        receiver.OnError(ex);
        Assert.That(captured, Is.EqualTo(ex));
    }

    [Test]
    public void EventReceiverT_OnCompleted_CallsOnCompleted() {
        bool completed = false;
        var receiver = new EventObserver<int>(_ => { }, null, () => completed = true);
        receiver.OnCompleted();
        Assert.That(completed, Is.True);
    }

    [Test]
    public void EventReceiverT_OnError_WithoutHandler_DoesNotThrow() {
        var receiver = new EventObserver<int>(_ => { });
        Assert.DoesNotThrow(() => receiver.OnError(new Exception()));
    }

    [Test]
    public void EventReceiverT_OnCompleted_WithoutHandler_DoesNotThrow() {
        var receiver = new EventObserver<int>(_ => { });
        Assert.DoesNotThrow(() => receiver.OnCompleted());
    }

    [Test]
    public void EventReceiverT_NullOnNext_DoesNotThrow() {
        Assert.DoesNotThrow(() => new EventObserver<int>(null));
    }

    [Test]
    public void EventReceiver_OnNext_CallsAction() {
        bool called = false;
        var receiver = new EventObserver(() => called = true);
        receiver.OnNext(default);
        Assert.That(called, Is.True);
    }

    [Test]
    public void EventReceiver_OnError_CallsOnError() {
        Exception captured = null;
        var receiver = new EventObserver(() => { }, ex => captured = ex);
        var ex = new InvalidOperationException("fail");
        receiver.OnError(ex);
        Assert.That(captured, Is.EqualTo(ex));
    }

    [Test]
    public void EventReceiver_OnCompleted_CallsOnCompleted() {
        bool completed = false;
        var receiver = new EventObserver(() => { }, null, () => completed = true);
        receiver.OnCompleted();
        Assert.That(completed, Is.True);
    }

    [Test]
    public void EventReceiver_OnError_WithoutHandler_DoesNotThrow() {
        var receiver = new EventObserver(() => { });
        Assert.DoesNotThrow(() => receiver.OnError(new Exception()));
    }

    [Test]
    public void EventReceiver_OnCompleted_WithoutHandler_DoesNotThrow() {
        var receiver = new EventObserver(() => { });
        Assert.DoesNotThrow(() => receiver.OnCompleted());
    }

    [Test]
    public void EventReceiver_NullOnNext_DoesNotThrow() {
        Assert.DoesNotThrow(() => new EventObserver<int>(null));
    }

    [Test]
    public void EventReceiverT_InvokesAllHandlersInOrder() {
        var calls = new List<string>();
        var receiver = new EventObserver<int>(
            x => calls.Add("next"),
            ex => calls.Add("error"),
            () => calls.Add("completed")
        );
        receiver.OnNext(1);
        receiver.OnError(new Exception());
        receiver.OnCompleted();
        Assert.That(calls, Is.EqualTo(new[] { "next", "error", "completed" }));
    }

    [Test]
    public void EventReceiver_InvokesAllHandlersInOrder() {
        var calls = new List<string>();
        var receiver = new EventObserver(
            () => calls.Add("next"),
            ex => calls.Add("error"),
            () => calls.Add("completed")
        );
        receiver.OnNext(default);
        receiver.OnError(new Exception());
        receiver.OnCompleted();
        Assert.That(calls, Is.EqualTo(new[] { "next", "error", "completed" }));
    }
}