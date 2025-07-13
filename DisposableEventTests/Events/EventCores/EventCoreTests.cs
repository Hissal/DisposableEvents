using DisposableEvents;
using DisposableEvents.Disposables;

namespace DisposableEventTests.EventFunctionalityTests.EventCores;

[TestFixture]
public class EventCoreTests {
    
    // Tests for EventCore functionality
    
    [Test]
    public void Subscribe_AddsObserver() {
        var core = new EventCore<int>();
        var obs1 = new TestObserver<int>();
        core.Subscribe(obs1);
        Assert.That(core.GetObservers(), Is.EquivalentTo(new[] { obs1 }));
    }

    [Test]
    public void Publish_SendsToAllSubscribers() {
        var core = new EventCore<int>();
        var obs1 = new TestObserver<int>();
        var obs2 = new TestObserver<int>();

        core.Subscribe(obs1);
        core.Subscribe(obs2);

        core.Publish(42);

        Assert.Multiple(() => {
            Assert.That(obs1.Received, Is.EquivalentTo(new[] { 42 }));
            Assert.That(obs2.Received, Is.EquivalentTo(new[] { 42 }));
        });
    }

    [Test]
    public void Observer_OnNextThrows_CallsOnError() {
        var core = new EventCore<int>();
        var error = new Exception("fail");
        var obs = new TestObserver<int>();
        var throwingObs = new ThrowingObserver<int>(error);

        core.Subscribe(throwingObs);
        core.Subscribe(obs);

        core.Publish(5);

        Assert.Multiple(() => {
            Assert.That(throwingObs.Error, Is.SameAs(error), "Observer that throws should have its error captured");
            Assert.That(obs.Received, Is.EquivalentTo(new[] { 5 }), "Non throwing observer should still receive the value");
        });
    }
    
    // Disposing tests
    
    [Test]
    public void Dispose_Disposes() {
        var core = new EventCore<int>();
        var isDisposedBefore = core.IsDisposed;
        core.Dispose();
        var isDisposedAfter = core.IsDisposed;
        Assert.Multiple(() => {
            Assert.That(isDisposedBefore, Is.False, "Before dispose should not be disposed");
            Assert.That(isDisposedAfter, Is.True, "After dispose should be disposed");
        });
    }
    
    [Test]
    public void DoubleDispose_IsSafe() {
        var core = new EventCore<int>();
        core.Dispose();
        Assert.DoesNotThrow(() => core.Dispose());
    }
    
    [Test]
    public void Dispose_CompletesAllObservers() {
        var core = new EventCore<int>();
        var obs1 = new TestObserver<int>();
        var obs2 = new TestObserver<int>();

        core.Subscribe(obs1);
        core.Subscribe(obs2);

        core.Dispose();

        Assert.Multiple(() => {
            Assert.That(obs1.Completed, Is.True);
            Assert.That(obs2.Completed, Is.True);
        });
    }
    
    [Test]
    public void DisposingSubscription_RemovesObserver() {
        var core = new EventCore<int>();
        var obs = new TestObserver<int>();
        var sub = core.Subscribe(obs);

        sub.Dispose();
        Assert.That(core.GetObservers(), Is.Empty, "Observer should be removed after subscription is disposed");
    }
    
    // Edge cases
    
    [Test]
    public void Publish_NullMessage_DoesNotThrow() {
        var core = new EventCore<string>();
        var obs = new TestObserver<string>();
        core.Subscribe(obs);

        Assert.DoesNotThrow(() => core.Publish(null));
        Assert.That(obs.Received, Is.EquivalentTo(new[] { (string)null }));
    }
    
    [Test]
    public void Subscribe_NullObserver_ThrowsArgumentNullException() {
        var core = new EventCore<int>();
        Assert.Throws<ArgumentNullException>(() => core.Subscribe(null));
    }
    
    [Test]
    public void Subscribe_NullObserverAfterDispose_ThrowsArgumentNullException() {
        var core = new EventCore<int>();
        core.Dispose();
        Assert.Throws<ArgumentNullException>(() => core.Subscribe(null));
    }
    
    [Test]
    public void Publish_AfterDispose_DoesNotThrow() {
        var core = new EventCore<int>();
        core.Dispose();
        
        Assert.DoesNotThrow(() => core.Publish(42), "Publishing after dispose should not throw");
    }
    
    [Test]
    public void Subscribe_AfterDispose_ObserverReceivesCompleted() {
        var core = new EventCore<int>();
        core.Dispose();
        
        var obs = new TestObserver<int>();
        core.Subscribe(obs);
        
        Assert.That(obs.Completed, Is.True, "Observer should receive completed after subscribing to disposed event");
    }
    
    [Test]
    public void Subscribe_AfterDispose_ReturnsEmptySubscription() {
        var core = new EventCore<int>();
        core.Dispose();
        
        var obs = new TestObserver<int>();
        var subscription = core.Subscribe(obs);
        
        Assert.That(subscription, Is.EqualTo(Disposable.Empty), "Subscription after dispose should be empty");
    }
    
    // Stress tests
    
    [Test]
    public void PubSub_Handles1000Subscribers() {
        var core = new EventCore<int>();
        var observers = new List<TestObserver<int>>();
        for (int i = 0; i < 1000; i++) {
            var obs = new TestObserver<int>();
            observers.Add(obs);
            core.Subscribe(obs);
        }
        core.Publish(42);
        foreach (var obs in observers) {
            Assert.That(obs.Received, Is.EquivalentTo(new[] { 42 }), "Each observer should receive the published value");
        }
    }
    
    [Test]
    public void PubSub_Handles1000SubscribersWithErrors() {
        var core = new EventCore<int>();
        var observers = new List<IObserver<int>>();
        var error = new Exception("Test error");
        
        for (int i = 0; i < 1000; i++) {
            IObserver<int> obs = i % 2 == 0 ? new TestObserver<int>() : new ThrowingObserver<int>(error);
            observers.Add(obs);
            core.Subscribe(obs);
        }
        
        core.Publish(42);
        
        foreach (var obs in observers) {
            switch (obs) {
                case ThrowingObserver<int> throwingObs:
                    Assert.That(throwingObs.Error, Is.SameAs(error), "Throwing observer should capture the error");
                    break;
                case TestObserver<int> testObs:
                    Assert.That(testObs.Received, Is.EquivalentTo(new[] { 42 }), "Non-throwing observer should receive the value");
                    break;
            }
        }
    }
}