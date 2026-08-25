using DisposableEvents;

namespace DisposableEvents.Tests.Filters;

[TestSubject(typeof(PredicateEventFilter<>))]
public class PredicateEventFilterTest {
    readonly Func<int, bool> predicate = Substitute.For<Func<int, bool>>();
    int message = 69;

    PredicateEventFilter<int> Sut => new(predicate);

    [Fact]
    public void Filter_WithTruePredicate_ReturnsPassed() {
        predicate.Invoke(Arg.Any<int>()).Returns(true);
        var result = Sut.Filter(ref message);
        result.Passed.Should().BeTrue();
    }
    
    [Fact]
    public void Filter_WithFalsePredicate_ReturnsBlocked() {
        predicate.Invoke(Arg.Any<int>()).Returns(false);
        var result = Sut.Filter(ref message);
        result.Blocked.Should().BeTrue();
    }
    
    [Fact]
    public void FilterOrder_ReturnsConstructorValue() {
        var sut = new PredicateEventFilter<int>(3, predicate);
        sut.FilterOrder.Should().Be(3);
    }

    [Fact]
    public void Filter_PassesMessageToPredicate() {
        Sut.Filter(ref message);
        predicate.Received(1).Invoke(message);
    }
    
    [Fact]
    public void FilterOrder_DefaultsToZero() {
        var sut = new PredicateEventFilter<int>(predicate);
        sut.FilterOrder.Should().Be(0);
    }
}