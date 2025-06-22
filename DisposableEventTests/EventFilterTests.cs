using System.Numerics;
using DisposableEvents;

namespace DisposableEventTests;

[TestFixture]
public class EventFilterTests {
    class TestObserver<T> : IObserver<T> {
        public List<T> Received { get; } = new();
        public bool Completed { get; private set; }
        public Exception Error { get; private set; }

        public void OnCompleted() {
            Completed = true;
        }

        public void OnError(Exception error) {
            Error = error;
        }

        public void OnNext(T value) => Received.Add(value);
    }

    [Test]
    public void PredicateEventFilter_FiltersEventsCorrectly() {
        var filter = new PredicateEventFilter<int>(x => x > 10);
        int value = 5;
        Assert.That(filter.FilterEvent(ref value), Is.False);
        value = 15;
        Assert.That(filter.FilterEvent(ref value), Is.True);
    }

    [Test]
    public void PredicateEventFilter_AllowsAllIfPredicateAlwaysTrue() {
        var filter = new PredicateEventFilter<int>(_ => true);
        int value = 0;
        Assert.That(filter.FilterEvent(ref value), Is.True);
        value = 100;
        Assert.That(filter.FilterEvent(ref value), Is.True);
    }

    [Test]
    public void PredicateEventFilter_BlocksAllIfPredicateAlwaysFalse() {
        var filter = new PredicateEventFilter<int>(_ => false);
        int value = 0;
        Assert.That(filter.FilterEvent(ref value), Is.False);
        value = 100;
        Assert.That(filter.FilterEvent(ref value), Is.False);
    }

    [Test]
    public void MultiEventFilter_AllFiltersMustPass() {
        var filter1 = new PredicateEventFilter<int>(x => x > 0);
        var filter2 = new PredicateEventFilter<int>(x => x % 2 == 0);
        var multi = new MultiEventFilter<int>(filter1, filter2);

        int value1 = 2;
        int value2 = -2;
        int value3 = 3;

        Assert.Multiple(() => {
            Assert.That(multi.FilterEvent(ref value1), Is.True); // >0 and even
            Assert.That(multi.FilterEvent(ref value2), Is.False); // not >0
            Assert.That(multi.FilterEvent(ref value3), Is.False); // not even
        });
    }

    [Test]
    public void MultiEventFilter_Empty_AllowsAll() {
        var multi = new MultiEventFilter<int>();
        int value = 123;
        Assert.That(multi.FilterEvent(ref value), Is.True);
    }

    [Test]
    public void FilteredEventReceiver_OnlyForwardsMatchingEvents() {
        var evt = new Event<int>();
        var observer = new TestObserver<int>();
        var filter = new PredicateEventFilter<int>(x => x % 2 == 1);

        evt.Subscribe(observer, filter);

        evt.Publish(1);
        evt.Publish(2);
        evt.Publish(3);

        Assert.That(observer.Received, Is.EquivalentTo(new[] { 1, 3 }));
    }

    [Test]
    public void FilteredEventReceiver_ForwardsOnErrorAndOnCompleted() {
        var evt = new Event<int>();
        bool completed = false;
        Exception captured = null;
        var filter = new PredicateEventFilter<int>(_ => true);

        evt.Subscribe(
            _ => { },
            ex => captured = ex,
            () => completed = true,
            filter
        );

        evt.Publish(1);
        evt.Dispose();


        // Simulate error
        var evt2 = new Event<int>();
        evt2.Subscribe(_ => throw new InvalidOperationException("fail"), ex => captured = ex, null, filter);
        evt2.Publish(1);

        Assert.Multiple(() => {
            Assert.That(completed, Is.True);
            Assert.That(captured, Is.TypeOf<InvalidOperationException>());
        });
    }

    [Test]
    public void PredicateEventFilter_FilterOnErrorAndCompleted_DefaultsToTrue() {
        var filter = new PredicateEventFilter<int>(_ => true);
        Assert.Multiple(() => {
            Assert.That(filter.FilterOnError(new Exception()), Is.True);
            Assert.That(filter.FilterOnCompleted(), Is.True);
        });
    }

    [Test]
    public void MultiEventFilter_FilterOnErrorAndCompleted_AllMustPass() {
        var filter1 = new TestFilter { OnErrorResult = true, OnCompletedResult = true };
        var filter2 = new TestFilter { OnErrorResult = true, OnCompletedResult = true };
        var multi1 = new MultiEventFilter<int>(filter1, filter2);

        var filter3 = new TestFilter { OnErrorResult = false, OnCompletedResult = true };
        var filter4 = new TestFilter { OnErrorResult = true, OnCompletedResult = false };
        var multi2 = new MultiEventFilter<int>(filter3, filter4);

        Assert.Multiple(() => {
            Assert.That(multi1.FilterOnError(new Exception()), Is.True);
            Assert.That(multi1.FilterOnCompleted(), Is.True);

            Assert.That(multi2.FilterOnError(new Exception()), Is.False);
            Assert.That(multi2.FilterOnCompleted(), Is.False);
        });
    }

    [Test]
    public void Filter_MutatesValueByRef() {
        var filter = new MutateValueFilter<int>(10);
        int value = 10;
        Assert.That(filter.FilterEvent(ref value), Is.True);
        Assert.That(value, Is.EqualTo(20));
    }

    [Test]
    public void MultiEventFilter_MutatesValueInCorrectOrder_UsingFilerOrder() {
        var filter1 = new MutateValueFilter<TestAddableString>("first") { FilterOrder = 1 };
        var filter2 = new MutateValueFilter<TestAddableString>("second") { FilterOrder = 2 };
        var filter3 = new MutateValueFilter<TestAddableString>("third") { FilterOrder = 3 };
        var multi1 = new MutateValueFilter<TestAddableString>("multi1") { FilterOrder = 1 };
        var multi2 = new MutateValueFilter<TestAddableString>("multi2") { FilterOrder = 2 };
        var multi12 = new MultiEventFilter<TestAddableString>(4, multi1, multi2);
        var sameOrder1 = new MutateValueFilter<TestAddableString>("same1") { FilterOrder = 0 };
        var sameOrder2 = new MutateValueFilter<TestAddableString>("same2") { FilterOrder = 0 };
        var multi = new MultiEventFilter<TestAddableString>(filter1, multi12, filter3, filter2, sameOrder1, sameOrder2);

        TestAddableString value = "";
        Assert.Multiple(() => {
            Assert.That(multi.FilterEvent(ref value), Is.True);
            // Should be "same1same2firstsecondthirdmulti1multi2"
            Assert.That(value.Value, Is.EqualTo("same1same2firstsecondthirdmulti1multi2"));
        });
    }

    [Test]
    public void MultiEventFilter_MutatesValueInCorrectOrder_UsingGivenOrder() {
        var filter1 = new MutateValueFilter<TestAddableString>("first");
        var filter2 = new MutateValueFilter<TestAddableString>("second");
        var filter3 = new MutateValueFilter<TestAddableString>("third");
        var multi = new MultiEventFilter<TestAddableString>(filter1, filter2, filter3);

        TestAddableString value = "";
        Assert.Multiple(() => {
            Assert.That(multi.FilterEvent(ref value), Is.True);
            // Should be "firstsecondthird"
            Assert.That(value.Value, Is.EqualTo("firstsecondthird"));
        });
    }

    struct TestAddableString : IAdditionOperators<TestAddableString, TestAddableString, TestAddableString> {
        public string Value { get; private set; }

        public static implicit operator string(TestAddableString value) => value.Value;
        public static implicit operator TestAddableString(string value) => new(value);

        public TestAddableString(string value) {
            Value = value;
        }

        public static TestAddableString operator +(TestAddableString left, TestAddableString right) {
            return new TestAddableString(left.Value + right.Value);
        }
    }

    class MutateValueFilter<T> : IEventFilter<T> where T : IAdditionOperators<T, T, T> {
        public T Add = default;
        public int FilterOrder { get; set; }

        public MutateValueFilter(T add) {
            Add = add;
        }

        public bool FilterEvent(ref T value) {
            value += Add;
            return true;
        }

        public bool FilterOnError(Exception error) => true;
        public bool FilterOnCompleted() => true;
    }

    class TestFilter : IEventFilter<int> {
        public bool OnErrorResult = true;
        public bool OnCompletedResult = true;
        public int FilterOrder { get; set; } = 0;
        public bool FilterEvent(ref int value) => true;
        public bool FilterOnError(Exception error) => OnErrorResult;
        public bool FilterOnCompleted() => OnCompletedResult;
    }
}