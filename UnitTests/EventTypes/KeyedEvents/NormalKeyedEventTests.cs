using DisposableEvents;

namespace UnitTests.EventTypes.KeyedEvents;

[TestFixture]
public class NormalKeyedEventTests {
    [Test]
    public void Publish_InvokesSubscriber_ForCorrectKey() {
        var evt = new KeyedEvent<string, int>();
        var obs = new TestObserver<int>();

        evt.Subscribe("foo", obs);
        evt.Publish("foo", 42);

        Assert.That(obs.LastValue, Is.EqualTo(42));
    }

    [Test]
    public void Publish_DoesNotInvokeSubscriber_ForOtherKey() {
        var evt = new KeyedEvent<string, int>();
        var obs = new TestObserver<int>();

        evt.Subscribe("foo", obs);
        evt.Publish("bar", 99);

        Assert.That(obs.LastValue, Is.EqualTo(0));
    }

    [Test]
    public void MultipleSubscribers_AllReceivePublishedValue_ForKey() {
        var evt = new KeyedEvent<string, int>();
        var obs1 = new TestObserver<int>();
        var obs2 = new TestObserver<int>();

        evt.Subscribe("foo", obs1);
        evt.Subscribe("foo", obs2);

        evt.Publish("foo", 123);

        Assert.Multiple(() => {
            Assert.That(obs1.LastValue, Is.EqualTo(123));
            Assert.That(obs2.LastValue, Is.EqualTo(123));
        });
    }

    [Test]
    public void DisposingSubscription_Unsubscribes_ForKey() {
        var evt = new KeyedEvent<string, int>();
        var obs = new TestObserver<int>();

        using (evt.Subscribe("foo", obs)) {
            evt.Publish("foo", 1);
            Assert.That(obs.LastValue, Is.EqualTo(1));
        }

        evt.Publish("foo", 2);
        Assert.That(obs.LastValue, Is.EqualTo(1));
    }

    [Test]
    public void Dispose_CallsOnCompleted_ForAllKeys() {
        var evt = new KeyedEvent<string, int>();
        var obs1 = new TestObserver<int>();
        var obs2 = new TestObserver<int>();

        evt.Subscribe("foo", obs1);
        evt.Subscribe("bar", obs2);

        evt.Dispose();

        Assert.Multiple(() => {
            Assert.That(obs1.Completed, Is.True);
            Assert.That(obs2.Completed, Is.True);
        });
    }
    
    [Test]
    public void Dispose_CallsOnCompleted_ForGivenKeys() {
        var evt = new KeyedEvent<string, int>();
        var obs1 = new TestObserver<int>();
        var obs2 = new TestObserver<int>();

        evt.Subscribe("foo", obs1);
        evt.Subscribe("bar", obs2);

        evt.Dispose("foo");

        Assert.Multiple(() => {
            Assert.That(obs1.Completed, Is.True);
            Assert.That(obs2.Completed, Is.False);
        });
    }
    
    [Test]
    public void SubscribeAfterDispose_ReceivesOnCompleted() {
        var evt = new KeyedEvent<string, int>();
        evt.Dispose();

        var obs = new TestObserver<int>();
        evt.Subscribe("foo", obs);

        Assert.That(obs.Completed, Is.True);
    }
    
    [Test]
    public void PublishWithNoSubscribers_DoesNotThrow() {
        var evt = new KeyedEvent<string, int>();
        Assert.DoesNotThrow(() => evt.Publish("foo", 42));
    }
}