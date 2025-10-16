using DisposableEvents;

namespace UnitTests.EventParts.EventFilters;

[TestFixture]
public class PredicateFilterTests {
    // Filter Event

    [Test]
    public void FilterEvent_Passes_WhenPredicateTrue() {
        var filter = new PredicateEventFilter<int>(x => x > 0);
        int value = 5;
        Assert.That(filter.Filter(ref value), Is.True);
    }

    [Test]
    public void FilterEvent_Blocks_WhenPredicateFalse() {
        var filter = new PredicateEventFilter<int>(x => x > 0);
        int value = -1;
        Assert.That(filter.Filter(ref value), Is.False);
    }

    [Test]
    public void FilterEvent_DefaultsToTrue_IfNullPredicate() {
        var filter = new PredicateEventFilter<int>();
        int value = 123;
        Assert.That(filter.Filter(ref value), Is.True);
    }

    // Filter OnCompleted

    [Test]
    public void FilterOnCompleted_Passes_WhenPredicateTrue() {
        var filter = new PredicateEventFilter<int>(completedFilter: () => true);
        Assert.That(filter.FilterOnCompleted(), Is.True);
    }

    [Test]
    public void FilterOnCompleted_Blocks_WhenPredicateFalse() {
        var filter = new PredicateEventFilter<int>(completedFilter: () => false);
        Assert.That(filter.FilterOnCompleted(), Is.False);
    }

    [Test]
    public void FilterOnCompleted_DefaultsToTrue_IfNull() {
        var filter = new PredicateEventFilter<int>();
        Assert.That(filter.FilterOnCompleted(), Is.True);
    }

    // Filter OnError

    [Test]
    public void FilterOnError_Passes_WhenPredicateTrue() {
        var filter = new PredicateEventFilter<int>(errorFilter: ex => true);
        Assert.That(filter.FilterOnError(new Exception()), Is.True);
    }

    [Test]
    public void FilterOnError_Blocks_WhenPredicateFalse() {
        var filter = new PredicateEventFilter<int>(errorFilter: ex => false);
        Assert.That(filter.FilterOnError(new Exception()), Is.False);
    }

    [Test]
    public void FilterOnError_DefaultsToTrue_IfNull() {
        var filter = new PredicateEventFilter<int>();
        Assert.That(filter.FilterOnError(new Exception()), Is.True);
    }

    // Filter Order

    [Test]
    public void FilterOrder_DefaultsToZero() {
        var filter = new PredicateEventFilter<int>();
        Assert.That(filter.FilterOrder, Is.EqualTo(0));
    }

    [Test]
    public void FilterOrder_CanBeSet() {
        var filter = new PredicateEventFilter<int>(42);
        Assert.That(filter.FilterOrder, Is.EqualTo(42));
    }

    // Composite filter

    [Test]
    public void WorksWithMultiFilter() {
        List<bool> received = [];

        var filter1 = new PredicateEventFilter<int>(x => {
            var result = x > 0;
            received.Add(result);
            return result;
        });
        var filter2 = new PredicateEventFilter<int>(x => {
            var result = x < 10;
            received.Add(result);
            return result;
        });
        var multiFilter = new CompositeEventFilter<int>(filter1, filter2);

        int value1 = 5;
        int value2 = 15;

        var result1 = multiFilter.Filter(ref value1);
        var result2 = multiFilter.Filter(ref value2);
        
        Assert.Multiple(() => {
            Assert.That(result1, Is.True);
            Assert.That(result2, Is.False);
            Assert.That(received, Is.EquivalentTo(new[] { true, true, true, false }));
        });
    }
}