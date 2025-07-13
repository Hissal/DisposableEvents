using DisposableEvents;

namespace DisposableEventTests;

[TestFixture]
public class BufferedEventTests {
    
    // Baseline tests
    
    [Test]
    public void Publish_InvokesSubscriber() {
        var evt = new BufferedEvent<int>();
        var obs = new TestObserver<int>();

        evt.Subscribe(obs);
        evt.Publish(42);

        Assert.That(obs.LastValue, Is.EqualTo(42));
    }

    [Test]
    public void OnError_HandlesError_WhenThrown() {
        var evt = new BufferedEvent<int>();
        var obs = new ThrowingObserver<int>(new InvalidOperationException("Test error"));

        evt.Subscribe(obs);

        Assert.DoesNotThrow(() => evt.Publish(42));
        Assert.That(obs.Error, Is.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void Dispose_CallsOnCompleted() {
        var evt = new BufferedEvent<int>();
        var obs = new TestObserver<int>();

        evt.Subscribe(obs);
        evt.Dispose();

        Assert.That(obs.Completed, Is.True);
    }

    [Test]
    public void DisposingSubscription_Unsubscribes() {
        var evt = new BufferedEvent<int>();
        var obs = new TestObserver<int>();

        using (evt.Subscribe(obs)) {
            evt.Publish(42);
            Assert.That(obs.LastValue, Is.EqualTo(42));
        }

        evt.Publish(100); // Should not invoke observer after disposal
        Assert.That(obs.LastValue, Is.EqualTo(42)); // Last value should remain unchanged
    }

    [Test]
    public void MultipleSubscribers_AllReceivePublishedValue() {
        var evt = new BufferedEvent<int>();
        var obs1 = new TestObserver<int>();
        var obs2 = new TestObserver<int>();

        evt.Subscribe(obs1);
        evt.Subscribe(obs2);

        evt.Publish(99);

        Assert.Multiple(() => {
            Assert.That(obs1.LastValue, Is.EqualTo(99));
            Assert.That(obs2.LastValue, Is.EqualTo(99));
        });
    }

    [Test]
    public void SubscribeAfterDispose_ReceivesOnCompleted() {
        var evt = new BufferedEvent<int>();
        var obs = new TestObserver<int>();

        evt.Dispose();
        evt.Subscribe(obs);

        Assert.That(obs.Completed, Is.True);
    }

    [Test]
    public void PublishWithNoSubscribers_DoesNotThrow() {
        var evt = new BufferedEvent<int>();
        Assert.DoesNotThrow(() => evt.Publish(42));
    }
    
    // Buffered behavior tests
    
    [Test]
    public void BufferedEvent_SubscribesWithPreviousMessage() {
        var evt = new BufferedEvent<int>();
        var obs = new TestObserver<int>();

        evt.Publish(1); // Publish before subscription
        evt.Subscribe(obs); // Subscribe after publishing

        Assert.That(obs.LastValue, Is.EqualTo(1)); // Should receive the last published value
    }
}