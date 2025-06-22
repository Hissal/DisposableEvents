using DisposableEvents;

namespace DisposableEventTests;

[TestFixture]
public class EventTests {
    [Test]
    public void Event_Publish_InvokesSubscriber() {
        var evt = new Event<int>();
        int received = 0;
        evt.Subscribe(x => received = x);

        evt.Publish(42);

        Assert.That(received, Is.EqualTo(42));
    }

    [Test]
    public void Event_Dispose_CallsOnCompleted() {
        var evt = new Event<int>();
        bool completed = false;
        evt.Subscribe(_ => { }, null, () => completed = true);

        evt.Dispose();

        Assert.That(completed, Is.True);
    }

    [Test]
    public void EventHandler_OnError_IsCalledOnException() {
        var evt = new Event<int>();
        Exception captured = null;
        evt.Subscribe(_ => throw new InvalidOperationException("fail"), ex => captured = ex);

        evt.Publish(1);

        Assert.That(captured, Is.TypeOf<InvalidOperationException>());
        Assert.That(captured.Message, Is.EqualTo("fail"));
    }

    [Test]
    public void Event_Subscription_CanBeDisposed() {
        var evt = new Event<int>();
        int callCount = 0;
        var sub = evt.Subscribe(_ => callCount++);

        evt.Publish(1);
        sub.Dispose();
        evt.Publish(2);

        Assert.That(callCount, Is.EqualTo(1));
    }

    [Test]
    public void Event_EmptyEvent_SubscribeAndPublish() {
        var evt = new Event();
        bool called = false;
        evt.Subscribe(() => called = true);

        evt.Publish();

        Assert.That(called, Is.True);
    }

    [Test]
    public void Event_MultipleSubscribers_AllReceivePublishedValue() {
        var evt = new Event<int>();
        int received1 = 0, received2 = 0;
        evt.Subscribe(x => received1 = x);
        evt.Subscribe(x => received2 = x);

        evt.Publish(123);

        Assert.That(received1, Is.EqualTo(123));
        Assert.That(received2, Is.EqualTo(123));
    }

    [Test]
    public void Event_FilteredSubscription_OnlyReceivesMatchingValues() {
        var evt = new Event<int>();
        int received = 0;
        var filter = new PredicateEventFilter<int>(x => x > 10);

        evt.Subscribe(x => received = x, filter);

        evt.Publish(5);
        Assert.That(received, Is.EqualTo(0));
        evt.Publish(20);
        Assert.That(received, Is.EqualTo(20));
    }

    [Test]
    public void Event_DisposedSubscription_StopsReceivingEvents() {
        var evt = new Event<int>();
        int received = 0;
        var sub = evt.Subscribe(x => received = x);

        evt.Publish(1);
        sub.Dispose();
        evt.Publish(2);

        Assert.That(received, Is.EqualTo(1));
    }

    [Test]
    public void Event_SubscribeAfterDispose_DoesNotThrow() {
        var evt = new Event<int>();
        evt.Dispose();
        Assert.DoesNotThrow(() => evt.Subscribe(_ => { }));
    }

    [Test]
    public void Event_PublishWithNoSubscribers_DoesNotThrow() {
        var evt = new Event<int>();
        Assert.DoesNotThrow(() => evt.Publish(99));
    }
}