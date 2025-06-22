using DisposableEvents;

namespace DisposableEventTests;

[TestFixture]
public class BufferedEventTests {
    [Test]
    public void BufferedEvent_SendsInitialValueOnSubscribe() {
        var evt = new BufferedEvent<string>("init");
        string received = null;
        evt.Subscribe(x => received = x);

        Assert.That(received, Is.EqualTo("init"));
    }

    [Test]
    public void BufferedEvent_Publish_UpdatesReceivedValue() {
        var evt = new BufferedEvent<int>(5);
        int received = 0;
        evt.Subscribe(x => received = x);

        evt.Publish(10);

        Assert.That(received, Is.EqualTo(10));
    }

    [Test]
    public void BufferedEvent_Publish_UpdatesBufferedValue() {
        var evt = new BufferedEvent<int>(5);
        int received = 0;
        int received2 = 0;

        evt.Subscribe(x => received = x);
        evt.Publish(10);
        evt.Subscribe(x => received2 = x);

        Assert.Multiple(() => {
            Assert.That(received, Is.EqualTo(10));
            Assert.That(received2, Is.EqualTo(10));
        });
    }

    [Test]
    public void BufferedEvent_MultipleSubscribers_ReceiveLatestValue() {
        var evt = new BufferedEvent<int>(7);
        int received1 = 0, received2 = 0;

        evt.Publish(42);

        evt.Subscribe(x => received1 = x);
        evt.Subscribe(x => received2 = x);

        Assert.That(received1, Is.EqualTo(42));
        Assert.That(received2, Is.EqualTo(42));
    }

    [Test]
    public void BufferedEvent_SubscriberReceivesUpdatesAfterSubscribe() {
        var evt = new BufferedEvent<string>("first");
        string received = null;
        evt.Subscribe(x => received = x);

        evt.Publish("second");
        Assert.That(received, Is.EqualTo("second"));

        evt.Publish("third");
        Assert.That(received, Is.EqualTo("third"));
    }

    [Test]
    public void BufferedEvent_FilteredSubscription_ReceivesOnlyFilteredValues() {
        var evt = new BufferedEvent<int>(1);
        int received = 0;
        var filter = new PredicateEventFilter<int>(x => x % 2 == 0);

        evt.Subscribe(x => received = x, filter); // Should not receive initial value (1)
        evt.Publish(3); // Should not receive
        Assert.That(received, Is.EqualTo(0));
        
        evt.Publish(4); // Should receive
        Assert.That(received, Is.EqualTo(4));
    }

    [Test]
    public void BufferedEvent_Dispose_PreventsFurtherNotifications() {
        var evt = new BufferedEvent<int>(10);
        int received = 0;
        evt.Subscribe(x => received = x);

        evt.Dispose();
        evt.Publish(99);

        Assert.That(received, Is.EqualTo(10));
    }

    [Test]
    public void BufferedEvent_AllowsNullOrDefaultValues() {
        var evt = new BufferedEvent<string>(null);
        string received = "not null";
        evt.Subscribe(x => received = x);

        Assert.That(received, Is.Null);

        evt.Publish("abc");
        Assert.That(received, Is.EqualTo("abc"));

        evt.Publish(null);
        Assert.That(received, Is.Null);
    }

    [Test]
    public void BufferedEvent_SubscribeAfterDispose_DoesNotThrow() {
        var evt = new BufferedEvent<int>(5);
        evt.Dispose();
        Assert.DoesNotThrow(() => evt.Subscribe(_ => { }));
    }
}