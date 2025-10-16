using DisposableEvents;

namespace UnitTests.EventParts.EventObservers;

[TestFixture]
public class FilteredObserverTests {
    
    // Predicate filter Tests (Relies on ´PredicateEventFilter´)
    
    [Test]
    public void OnNext_Passes_WhenFilterAllows() {
        var observer = new TestObserver<int>();
        var filter = new PredicateEventFilter<int>(x => true);
        var filtered = new FilteredEventHandler<int>(observer, filter);

        filtered.OnNext(5);
  
        Assert.That(observer.Received, Is.EquivalentTo(new[] { 5 }));
    }

    [Test]
    public void OnNext_DoesNotPass_WhenFilterBlocks() {
        var observer = new TestObserver<int>();
        var filter = new PredicateEventFilter<int>(x => false);
        var filtered = new FilteredEventHandler<int>(observer, filter);

        filtered.OnNext(42);

        Assert.That(observer.Received, Is.Empty);
    }

    [Test]
    public void OnError_Passes_WhenFilterAllows() {
        var observer = new TestObserver<int>();
        var filter = new PredicateEventFilter<int>(errorFilter: ex => true);
        var filtered = new FilteredEventHandler<int>(observer, filter);

        var ex = new Exception("fail");
        filtered.OnError(ex);

        Assert.That(observer.Error, Is.EqualTo(ex));
    }

    [Test]
    public void OnError_DoesNotPass_WhenFilterBlocks() {
        var observer = new TestObserver<int>();
        var filter = new PredicateEventFilter<int>(errorFilter: ex => false);
        var filtered = new FilteredEventHandler<int>(observer, filter);

        filtered.OnError(new Exception("fail"));

        Assert.That(observer.Error, Is.Null);
    }

    [Test]
    public void OnCompleted_Passes_WhenFilterAllows() {
        var observer = new TestObserver<int>();
        var filter = new PredicateEventFilter<int>(completedFilter: () => true);
        var filtered = new FilteredEventHandler<int>(observer, filter);

        filtered.OnCompleted();

        Assert.That(observer.Completed, Is.True);
    }

    [Test]
    public void OnCompleted_DoesNotPass_WhenFilterBlocks() {
        var observer = new TestObserver<int>();
        var filter = new PredicateEventFilter<int>(completedFilter: () => false);
        var filtered = new FilteredEventHandler<int>(observer, filter);

        filtered.OnCompleted();

        Assert.That(observer.Completed, Is.False);
    }

    // Mutating filter test (Relies on ´MutatingEventFilter´)
    
    [Test]
    public void FilterEvent_CanMutateValue() {
        var observer = new TestObserver<int>();
        var filter = new ValueMutatorFilter<int>(x => x * 2);
        var filtered = new FilteredEventHandler<int>(observer, filter);

        filtered.OnNext(3);

        Assert.That(observer.Received, Is.EquivalentTo(new[] { 6 }));
    }

    // Throws tests
    
    [Test]
    public void ThrowsOnNullObserver() {
        var filter = new PredicateEventFilter<int>(x => true);
        Assert.Throws<ArgumentNullException>(() => new FilteredEventHandler<int>(null!, filter));
    }

    [Test]
    public void ThrowsOnNullFilter() {
        var observer = new TestObserver<int>();
        Assert.Throws<ArgumentNullException>(() => new FilteredEventHandler<int>(observer, null!));
    }
}