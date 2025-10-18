using DisposableEvents;
using DisposableEvents.Disposables;

namespace UnitTests.EventParts.EventCores;

[TestFixture]
public class FuncCoreTests {
    // Tests for EventFuncCore functionality

    [Test]
    public void Subscribe_AddsObserver() {
        var core = new FuncCore<int, int>();
        var obs1 = new TestFuncHandler<int, int>();

        core.Subscribe(obs1);

        Assert.That(core.Observers, Is.EquivalentTo(new[] { obs1 }));
    }

    [Test]
    public void Publish_SendsToAllSubscribers() {
        var core = new FuncCore<int, int>();
        var obs1 = new TestFuncHandler<int, int>();
        var obs2 = new TestFuncHandler<int, int>();

        core.Subscribe(obs1);
        core.Subscribe(obs2);
        core.Publish(42, FuncResultBehavior.ReturnLastSuccess);

        Assert.Multiple(() => {
            Assert.That(obs1.LastValue, Is.EqualTo(42));
            Assert.That(obs2.LastValue, Is.EqualTo(42));
        });
    }

    [Test]
    public void Observer_OnNextThrows_CallsOnErrorAndContinues() {
        var core = new FuncCore<int, int>();
        var error = new Exception("fail");
        var obs = new TestFuncHandler<int, int>();
        var throwingObs = new ThrowingFuncHandler<int, int>(error);

        core.Subscribe(throwingObs);
        core.Subscribe(obs);
        core.Publish(5, FuncResultBehavior.ReturnLastSuccess);

        Assert.Multiple(() => {
            Assert.That(throwingObs.Error, Is.SameAs(error), "Observer that throws should have its error captured");
            Assert.That(obs.LastValue, Is.EqualTo(5), "Non throwing observer should still receive the value");
        });
    }

    // Disposing tests

    [Test]
    public void Dispose_Disposes() {
        var core = new FuncCore<int, int>();

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
        var core = new FuncCore<int, int>();
        core.Dispose();
        Assert.DoesNotThrow(() => core.Dispose());
    }

    [Test]
    public void Dispose_CompletesAllObservers() {
        var core = new FuncCore<int, int>();
        var obs1 = new TestFuncHandler<int, int>();
        var obs2 = new TestFuncHandler<int, int>();

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
        var core = new FuncCore<int, int>();
        var obs = new TestFuncHandler<int, int>();

        var sub = core.Subscribe(obs);
        sub.Dispose();

        Assert.That(core.Observers, Is.Empty, "Observer should be removed after subscription is disposed");
    }

    // Edge cases

    [Test]
    public void Publish_NullMessage_DoesNotThrow() {
        var core = new FuncCore<string, int>();
        var obs = new TestFuncHandler<string, int>();

        core.Subscribe(obs);

        Assert.DoesNotThrow(() => core.Publish(null, FuncResultBehavior.ReturnLastSuccess));
        Assert.That(obs.Received, Is.EquivalentTo(new[] { (string)null }));
    }

    [Test]
    public void Subscribe_NullObserver_ThrowsArgumentNullException() {
        var core = new FuncCore<int, int>();
        Assert.Throws<ArgumentNullException>(() => core.Subscribe(null));
    }

    [Test]
    public void Subscribe_NullObserverAfterDispose_ThrowsArgumentNullException() {
        var core = new FuncCore<int, int>();
        core.Dispose();
        Assert.Throws<ArgumentNullException>(() => core.Subscribe(null));
    }

    [Test]
    public void Publish_AfterDispose_DoesNotThrow() {
        var core = new FuncCore<int, int>();
        core.Dispose();

        Assert.DoesNotThrow(() => core.Publish(42, FuncResultBehavior.ReturnLastSuccess),
            "Publishing after dispose should not throw");
    }

    [Test]
    public void Subscribe_AfterDispose_ObserverReceivesCompleted() {
        var core = new FuncCore<int, int>();
        core.Dispose();

        var obs = new TestFuncHandler<int, int>();
        core.Subscribe(obs);

        Assert.That(obs.Completed, Is.True, "Observer should receive completed after subscribing to disposed event");
    }

    [Test]
    public void Subscribe_AfterDispose_ReturnsEmptySubscription() {
        var core = new FuncCore<int, int>();
        core.Dispose();

        var obs = new TestFuncHandler<int, int>();
        var subscription = core.Subscribe(obs);

        Assert.That(subscription, Is.EqualTo(Disposable.Empty), "Subscription after dispose should be empty");
    }

    // Stress tests

    [Test]
    public void PubSub_Handles1000Subscribers() {
        var core = new FuncCore<int, int>();
        var observers = new List<TestFuncHandler<int, int>>();

        for (int i = 0; i < 1000; i++) {
            var obs = new TestFuncHandler<int, int>();
            observers.Add(obs);
            core.Subscribe(obs);
        }

        core.Publish(42, FuncResultBehavior.ReturnLastSuccess);

        foreach (var obs in observers) {
            Assert.That(obs.LastValue, Is.EqualTo(42), "Each observer should receive the published value");
        }
    }

    [Test]
    public void PubSub_Handles1000SubscribersWithErrors() {
        var core = new FuncCore<int, int>();
        var observers = new List<IFuncHandler<int, int>>();
        var error = new Exception("Test error");

        for (int i = 0; i < 1000; i++) {
            IFuncHandler<int, int> obs =
                i % 2 == 0 ? new TestFuncHandler<int, int>() : new ThrowingFuncHandler<int, int>(error);
            observers.Add(obs);
            core.Subscribe(obs);
        }

        core.Publish(42, FuncResultBehavior.ReturnLastSuccess);

        foreach (var obs in observers) {
            switch (obs) {
                case ThrowingFuncHandler<int, int> throwingObs:
                    Assert.That(throwingObs.Error, Is.SameAs(error), "Throwing observer should capture the error");
                    break;
                case TestFuncHandler<int, int> testObs:
                    Assert.That(testObs.LastValue, Is.EqualTo(42), "Non-throwing observer should receive the value");
                    break;
            }
        }
    }

    // Func result behavior tests

    [Test]
    public void Publish_ReturnsLastSuccess() {
        var core = new FuncCore<int, int>();
        var obs1 = new TestFuncHandler<int, int>(5, true);
        var obs2 = new TestFuncHandler<int, int>(10, false);

        core.Subscribe(obs1);
        core.Subscribe(obs2);
        var result = core.Publish(42, FuncResultBehavior.ReturnLastSuccess);
        
        Assert.Multiple(() => {
            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value, Is.EqualTo(5), "Should return last successful value");
        });
    }
    
    [Test]
    public void Publish_ReturnsFirstSuccess() {
        var core = new FuncCore<int, int>();
        var obs1 = new TestFuncHandler<int, int>(5, true);
        var obs2 = new TestFuncHandler<int, int>(10, false);

        core.Subscribe(obs1);
        core.Subscribe(obs2);
        var result = core.Publish(42, FuncResultBehavior.ReturnFirstSuccess);
        
        Assert.Multiple(() => {
            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value, Is.EqualTo(5), "Should return first successful value");
            Assert.That(obs2.Received, Has.Count.EqualTo(1), "Second observer should be called");
        });
    }
    
    [Test]
    public void Publish_ReturnsFirstSuccessAndStops() {
        var core = new FuncCore<int, int>();
        var obs1 = new TestFuncHandler<int, int>(5, true);
        var obs2 = new TestFuncHandler<int, int>(10, false);

        core.Subscribe(obs1);
        core.Subscribe(obs2);
        var result = core.Publish(42, FuncResultBehavior.ReturnFirstSuccessAndStop);
        
        Assert.Multiple(() => {
            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value, Is.EqualTo(5), "Should return first successful value");
            Assert.That(obs2.Received, Has.Count.EqualTo(0), "Second observer should not be called");
        });
    }
    
    [Test]
    public void Publish_ReturnsLast() {
        var core = new FuncCore<int, int>();
        var obs1 = new TestFuncHandler<int, int>(5, true);
        var obs2 = new TestFuncHandler<int, int>(10, false);

        core.Subscribe(obs1);
        core.Subscribe(obs2);
        var result = core.Publish(42, FuncResultBehavior.ReturnLast);
        
        Assert.Multiple(() => {
            Assert.That(result.HasValue, Is.False);
            Assert.That(result.Value, Is.EqualTo(10), "Should return last value regardless of success");
        });
    }
    
    [Test]
    public void Publish_ReturnsFirst() {
        var core = new FuncCore<int, int>();
        var obs1 = new TestFuncHandler<int, int>(5, true);
        var obs2 = new TestFuncHandler<int, int>(10, false);

        core.Subscribe(obs1);
        core.Subscribe(obs2);
        var result = core.Publish(42, FuncResultBehavior.ReturnFirst);
        
        Assert.Multiple(() => {
            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value, Is.EqualTo(5), "Should return first value regardless of success");
            Assert.That(obs2.Received, Has.Count.EqualTo(1), "Second observer should be called");
        });
    }
    
    [Test]
    public void Publish_ReturnsFirstAndStops() {
        var core = new FuncCore<int, int>();
        var obs1 = new TestFuncHandler<int, int>(5, true);
        var obs2 = new TestFuncHandler<int, int>(10, false);

        core.Subscribe(obs1);
        core.Subscribe(obs2);
        var result = core.Publish(42, FuncResultBehavior.ReturnFirstAndStop);
        
        Assert.Multiple(() => {
            Assert.That(result.HasValue, Is.True);
            Assert.That(result.Value, Is.EqualTo(5), "Should return first value regardless of success");
            Assert.That(obs2.Received, Has.Count.EqualTo(0), "Second observer should not be called");
        });
    }
    
    [Test]
    public void PublishEnumerator_None() {
        var core = new FuncCore<int, int>();
        var obs1 = new TestFuncHandler<int, int>(5, true);
        var obs2 = new TestFuncHandler<int, int>(10, false);
        var throwingObs = new ThrowingFuncHandler<int, int>(new Exception("Test error"));

        core.Subscribe(obs1);
        core.Subscribe(obs2);
        core.Subscribe(throwingObs);
        var results = core.PublishEnumerator(42, FuncResultEnumerationBehaviorFlags.None).ToArray();
        
        Assert.Multiple(() => {
            Assert.That(results, Has.Length.EqualTo(3), "Should return all results from observers");
            
            Assert.That(results[0].Value, Is.EqualTo(5), "First observer should return its value");
            Assert.That(results[0].HasValue, Is.True, "First observer should return success");
            
            Assert.That(results[1].Value, Is.EqualTo(10), "Second observer should return its value");
            Assert.That(results[1].HasValue, Is.False, "Second observer should return failure");
            
            Assert.That(results[2].HasValue, Is.False, "Throwing observer should return failure");
        });
    }
    
    [Test]
    public void PublishEnumerator_SkipsFailures() {
        var core = new FuncCore<int, int>();
        var obs1 = new TestFuncHandler<int, int>(5, true);
        var obs2 = new TestFuncHandler<int, int>(10, false);

        core.Subscribe(obs1);
        core.Subscribe(obs2);
        
        var results = core.PublishEnumerator(42, FuncResultEnumerationBehaviorFlags.SkipFailures).ToArray();
        
        Assert.Multiple(() => {
            Assert.That(results, Has.Length.EqualTo(1), "Should skip failing observers");
            Assert.That(results[0].Value, Is.EqualTo(5), "Only successful observer should return its value");
        });
    }
    
    // [Test]
    // public void PublishEnumerator_ReturnsAllResultsWithErrors() {
    //     var core = new EventFuncCore<int, int>();
    //     var obs1 = new TestFuncObserver<int, int>(5, true);
    //     var obs2 = new ThrowingFuncObserver<int, int>(new Exception("Test error"));
    //
    //     core.Subscribe(obs1);
    //     core.Subscribe(obs2);
    //     
    //     var results = core.PublishEnumerator(42, FuncResultEnumerationBehaviorFlags.ReturnAll).ToArray();
    //     
    //     Assert.Multiple(() => {
    //         Assert.That(results, Has.Length.EqualTo(2), "Should return all results including errors");
    //         Assert.That(results[0].Value, Is.EqualTo(5), "First observer should return its value");
    //         Assert.That(results[1].IsSuccess, Is.False, "Second observer should return failure");
    //         Assert.That(results[1].Error.Message, Is.EqualTo("Test error"), "Should capture error from second observer");
    //     });
    // }
}