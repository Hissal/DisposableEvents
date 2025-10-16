using DisposableEvents;

namespace UnitTests.EventParts.EventFilters;

[TestFixture]
public class MutatingFilterTests {
    [Test]
    public void FilterEvent_MutatesValue() {
        var expected = 10;
        var filter = new MutatingEventFilter<int>(x => expected);
        int value = 5;
        filter.Filter(ref value);
        Assert.That(value, Is.EqualTo(expected));
    }

    [Test]
    public void FilterEvent_NoOp_WhenNullFunc() {
        var filter = new MutatingEventFilter<int>(null!);
        int value = 7;
        filter.Filter(ref value);
        Assert.That(value, Is.EqualTo(7));
    }

    [Test]
    public void FilterEvent_AlwaysTrue() {
        var filter = new MutatingEventFilter<int>(x => x + 1);
        int value = 3;
        Assert.That(filter.Filter(ref value), Is.True);
    }

    [Test]
    public void FilterOnCompleted_AlwaysTrue() {
        var filter = new MutatingEventFilter<int>(x => x);
        Assert.That(filter.FilterOnCompleted(), Is.True);
    }

    [Test]
    public void FilterOnError_AlwaysTrue() {
        var filter = new MutatingEventFilter<int>(x => x);
        Assert.That(filter.FilterOnError(new Exception()), Is.True);
    }

    // Filter order
    
    [Test]
    public void FilterOrder_DefaultsToZero() {
        var filter = new MutatingEventFilter<int>(x => x);
        Assert.That(filter.FilterOrder, Is.EqualTo(0));
    }

    [Test]
    public void FilterOrder_CanBeSet() {
        var filter = new MutatingEventFilter<int>(42, x => x);
        Assert.That(filter.FilterOrder, Is.EqualTo(42));
    }

    // Composite filter
    
    [Test]
    public void WorksWithMultiFilter() {
        List<int> received = [];
        var filter1 = new MutatingEventFilter<int>(x => {
            received.Add(x);
            return x + 1;
        });
        var filter2 = new MutatingEventFilter<int>(x => {
            received.Add(x);
            return x + 1;
        });
        var filter3 = new MutatingEventFilter<int>(x => {
            received.Add(x);
            return x + 1;
        });
        var multiFilter = new CompositeEventFilter<int>(filter1, filter2, filter3);
        
        int value = 1;
        multiFilter.Filter(ref value);
        
        Assert.Multiple(() => {
            Assert.That(value, Is.EqualTo(4));
            Assert.That(received, Is.EqualTo(new List<int> { 1, 2, 3 }));
        });
    }
}