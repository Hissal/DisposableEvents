using DisposableEvents;

namespace UnitTests.EventTypes;

[TestFixture]
public class EventExtensionTests {
    [Test]
    public void Subscribe_Action_ReceivesPublishedValue() {
        var evt = new Event<int>();
        int received = 0;
        evt.Subscribe(x => received = x);
        evt.Publish(123);
        Assert.That(received, Is.EqualTo(123));
    }

    [Test]
    public void Subscribe_ActionWithPredicate_OnlyReceivesPassingValues() {
        var evt = new Event<int>();
        int received = 0;
        evt.Subscribe(x => received = x, v => v > 10);
        evt.Publish(20);
        evt.Publish(5);
        Assert.That(received, Is.EqualTo(20));
        
        evt.Subscribe(x => received = x);
    }

    [Test]
    public void Subscribe_ActionWithOnErrorAndOnComplete_HandlesErrorAndCompletion() {
        var evt = new Event<int>();
        Exception? captured = null;
        bool completed = false;
        evt.Subscribe(
            x => throw new InvalidOperationException("fail"),
            ex => captured = ex,
            () => completed = true
        );
        evt.Publish(1);
        evt.Dispose();
        Assert.That(captured, Is.InstanceOf<InvalidOperationException>());
        Assert.That(completed, Is.True);
    }

    [Test]
    public void Subscribe_ClosureEventObserver_ReceivesValueWithClosure() {
        var evt = new Event<int>();
        int sum = 0;
        evt.Subscribe(10, (closure, value) => sum = closure + value);
        evt.Publish(5);
        Assert.That(sum, Is.EqualTo(15));
    }
}