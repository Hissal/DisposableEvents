using DisposableEvents;

namespace DisposableEventTests;

[TestFixture]
public class ObservableEventTests {
    class TestObserver<T> : IObserver<T> {
        public List<T> Received { get; } = new();
        public bool Completed { get; private set; }
        public Exception Error { get; private set; }
        public void OnCompleted() { Completed = true; }
        public void OnError(Exception error) { Error = error; }
        public void OnNext(T value) => Received.Add(value);
    }

    [Test]
    public void Publish_ForwardsToObserver() {
        var normalEvent = new Event<int>();
        var observable = new ObservableEvent<int>(normalEvent);
        var observer = new TestObserver<int>();

        using (observable.Subscribe(observer)) {
            observable.Publish(42);
            observable.Publish(100);
        }

        Assert.That(observer.Received, Is.EquivalentTo(new[] { 42, 100 }));
    }

    [Test]
    public void Dispose_DisposesWrappedEvent() {
        var normalEvent = new Event<int>();
        var observable = new ObservableEvent<int>(normalEvent);
        
        // After dispose, publishing should not notify observers
        var observer = new TestObserver<int>();
        observable.Subscribe(observer);
        observable.Publish(1);
        observable.Dispose();
        
        observable.Publish(2);

        Assert.That(observer.Received, Is.EquivalentTo(new[] { 1 }));
    }

    [Test]
    public void Subscribe_WithFilters_AppliesFilters() {
        var normalEvent = new Event<int>();
        var observable = new ObservableEvent<int>(normalEvent);
        var observer = new TestObserver<int>();

        var filter = new PredicateEventFilter<int>(x => x > 10);

        using (observable.Subscribe(observer, filter)) {
            observable.Publish(5);
            observable.Publish(15);
        }

        Assert.That(observer.Received, Is.EquivalentTo(new[] { 15 }));
    }

    [Test]
    public void ToObservableEvent_Extension_WrapsEvent() {
        var normalEvent = new Event<string>();
        var observable = normalEvent.ToObservableEvent();

        var observer = new TestObserver<string>();
        using (observable.Subscribe(observer)) {
            observable.Publish("hello");
        }

        Assert.That(observer.Received, Is.EquivalentTo(new[] { "hello" }));
    }

    [Test]
    public void MultipleObservers_AllReceivePublishedValues() {
        var normalEvent = new Event<int>();
        var observable = new ObservableEvent<int>(normalEvent);
        var observer1 = new TestObserver<int>();
        var observer2 = new TestObserver<int>();

        using (observable.Subscribe(observer1))
        using (observable.Subscribe(observer2)) {
            observable.Publish(1);
            observable.Publish(2);
        }

        Assert.That(observer1.Received, Is.EquivalentTo(new[] { 1, 2 }));
        Assert.That(observer2.Received, Is.EquivalentTo(new[] { 1, 2 }));
    }

    [Test]
    public void Observer_Dispose_StopsReceivingEvents() {
        var normalEvent = new Event<int>();
        var observable = new ObservableEvent<int>(normalEvent);
        var observer = new TestObserver<int>();

        var sub = observable.Subscribe(observer);
        observable.Publish(10);
        sub.Dispose();
        observable.Publish(20);

        Assert.That(observer.Received, Is.EquivalentTo(new[] { 10 }));
    }

    [Test]
    public void Observer_OnError_IsCalledOnException() {
        var normalEvent = new Event<int>();
        var observable = new ObservableEvent<int>(normalEvent);

        var observer = new TestObserver<int>();
        observable.Subscribe(new EventReceiver<int>(_ => throw new InvalidOperationException("fail"), ex => observer.OnError(ex)));

        observable.Publish(5);

        Assert.That(observer.Error, Is.TypeOf<InvalidOperationException>());
        Assert.That(observer.Error.Message, Is.EqualTo("fail"));
    }

    [Test]
    public void Observer_OnCompleted_IsCalledOnDispose() {
        var normalEvent = new Event<int>();
        var observable = new ObservableEvent<int>(normalEvent);

        var observer = new TestObserver<int>();
        observable.Subscribe(observer);

        observable.Dispose();

        Assert.That(observer.Completed, Is.True);
    }

    // [Test]
    // public void SubscribeAfterDispose_DoesNotThrowAndDoesNotReceiveEvents() {
    //     var normalEvent = new Event<int>();
    //     var observable = new ObservableEvent<int>(normalEvent);
    //     observable.Dispose();
    //
    //     var observer = new TestObserver<int>();
    //     Assert.DoesNotThrow(() => observable.Subscribe(observer));
    //     observable.Publish(99);
    //
    //     Assert.That(observer.Received, Is.Empty);
    // }

    [Test]
    public void PublishWithNoObservers_DoesNotThrow() {
        var normalEvent = new Event<int>();
        var observable = new ObservableEvent<int>(normalEvent);
        Assert.DoesNotThrow(() => observable.Publish(123));
    }

    [Test]
    public void FilteredObserver_DoesNotReceiveNonMatchingValues() {
        var normalEvent = new Event<int>();
        var observable = new ObservableEvent<int>(normalEvent);
        var observer = new TestObserver<int>();
        var filter = new PredicateEventFilter<int>(x => x % 2 == 0);

        using (observable.Subscribe(observer, filter)) {
            observable.Publish(1);
            observable.Publish(2);
            observable.Publish(3);
            observable.Publish(4);
        }

        Assert.That(observer.Received, Is.EquivalentTo(new[] { 2, 4 }));
    }
}