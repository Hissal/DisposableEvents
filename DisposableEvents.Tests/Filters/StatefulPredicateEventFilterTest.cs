using DisposableEvents;

namespace DisposableEvents.Tests.Filters;

[TestSubject(typeof(PredicateEventFilter<,>))]
public class StatefulPredicateEventFilterTest {
    readonly Func<int, int, bool> predicate = Substitute.For<Func<int, int, bool>>();
    int message = 69;
    const int c_state = 42;

    PredicateEventFilter<int, int> Sut => new(c_state, predicate);

    [Fact]
    public void Filter_WithTruePredicate_ReturnsPassed() {
        predicate.Invoke(Arg.Any<int>(), Arg.Any<int>()).Returns(true);
        var result = Sut.Filter(ref message);
        result.Passed.Should().BeTrue();
    }
    
    [Fact]
    public void Filter_WithFalsePredicate_ReturnsBlocked() {
        predicate.Invoke(Arg.Any<int>(), Arg.Any<int>()).Returns(false);
        var result = Sut.Filter(ref message);
        result.Blocked.Should().BeTrue();
    }
    
    [Fact]
    public void FilterOrder_ReturnsConstructorValue() {
        var sut = new PredicateEventFilter<int, int>(c_state, 3, predicate);
        sut.FilterOrder.Should().Be(3);
    }

    [Fact]
    public void Filter_PassesMessageToPredicate() {
        Sut.Filter(ref message);
        predicate.Received(1).Invoke(Arg.Any<int>(), message);
    }
    
    [Fact]
    public void Filter_PassesStateToPredicate() {
        Sut.Filter(ref message);
        predicate.Received(1).Invoke(c_state, Arg.Any<int>());
    }
    
    [Fact]
    public void FilterOrder_DefaultsToZero() {
        var sut = new PredicateEventFilter<int, int>(c_state, predicate);
        sut.FilterOrder.Should().Be(0);
    }
}