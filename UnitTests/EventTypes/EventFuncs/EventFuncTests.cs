using DisposableEvents;

namespace UnitTests.EventTypes.EventFuncs;

[TestFixture]
public class EventFuncTests {
    
    // Baseline tests
    [Test]
    public void Publish_InvokesSubscriber() {
        var evt = new EventFunc<int, int>();
        var obs = new TestFuncObserver<int, int>(5);

        evt.Subscribe(obs);
        evt.Publish(10);
        
        Assert.That(obs.LastValue, Is.EqualTo(10));
    }
    
    [Test]
    public void OnError_HandlesError_WhenThrown() {
        var evt = new EventFunc<int, int>();
        var obs = new ThrowingFuncObserver<int, int>(new TestException("Test error"));

        evt.Subscribe(obs);

        Assert.DoesNotThrow(() => evt.Publish(42));
        Assert.That(obs.Error, Is.InstanceOf<TestException>());
    }

    [Test]
    public void Dispose_CallsOnCompleted() {
        var evt = new EventFunc<int, int>();
        var obs = new TestFuncObserver<int, int>(10);

        evt.Subscribe(obs);
        evt.Dispose();

        Assert.That(obs.Completed, Is.True);
    }

    [Test]
    public void DisposingSubscription_Unsubscribes() {
        var evt = new EventFunc<int, int>();
        var obs = new TestFuncObserver<int, int>();

        using (evt.Subscribe(obs)) {
            evt.Publish(42);
            Assert.That(obs.LastValue, Is.EqualTo(42));
        }

        evt.Publish(100); // Should not invoke observer after disposal
        Assert.That(obs.LastValue, Is.EqualTo(42)); // Last value should remain unchanged
    }

    [Test]
    public void MultipleSubscribers_AllReceivePublishedValue() {
        var evt = new EventFunc<int, int>();
        var obs1 = new TestFuncObserver<int, int>();
        var obs2 = new TestFuncObserver<int, int>();

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
        var evt = new DisposableEvent<int>();
        var obs = new TestObserver<int>();

        evt.Dispose();
        evt.Subscribe(obs);

        Assert.That(obs.Completed, Is.True);
    }

    [Test]
    public void PublishWithNoSubscribers_DoesNotThrow() {
        var evt = new EventFunc<int, int>();
        Assert.DoesNotThrow(() => evt.Publish(42));
    }

    [Test]
    public void Subscribe_AppliesFilters() {
        var evt = new EventFunc<int, int>();
        var obs = new TestFuncObserver<int, int>();
        var filter = new TestFilter<int>();

        evt.Subscribe(obs, filter);
        evt.Publish(10);
        
        Assert.That(filter.LastValue, Is.EqualTo(10));
    }
    
    // Func tests
    
    [Test]
    public void Publish_ReturnsResult() {
        var evt = new EventFunc<int, int>();
        var obs = new TestFuncObserver<int, int>(5);

        evt.Subscribe(obs);
        var result = evt.Publish(10);
        
        Assert.That(result.Value, Is.EqualTo(5));
    }
    
    [Test]
    public void PublishEnumerator_ReturnsEnumeratedResults() {
        var evt = new EventFunc<int, int>();
        var obs1 = new TestFuncObserver<int, int>(5);
        var obs2 = new TestFuncObserver<int, int>(10);
        var obs3 = new TestFuncObserver<int, int>(15);

        evt.Subscribe(obs1);
        evt.Subscribe(obs2);
        evt.Subscribe(obs3);
        var result = evt.PublishEnumerator(20).Select(r => r.Value).ToArray();
        
        Assert.That(result, Is.EquivalentTo(new[] { 5, 10, 15 }));
    }
}