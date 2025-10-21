using DisposableEvents;

namespace DisposableEvents.Tests.Filters;

[TestSubject(typeof(VoidPredicateEventFilter))]
public class VoidPredicateEventFilterTest {
    readonly Func<bool> predicate = Substitute.For<Func<bool>>();
    Void message = Void.Value;
    VoidPredicateEventFilter Sut => new(predicate);

    [Fact]
    public void Should_ReturnFilterResultPassed_WhenPredicateTrue() {
        predicate.Invoke().Returns(true);
        var result = Sut.Filter(ref message);
        result.Passed.Should().BeTrue();
    }
    
    [Fact]
    public void Should_ReturnFilterResultBlocked_WhenPredicateFalse() {
        predicate.Invoke().Returns(false);
        var result = Sut.Filter(ref message);
        result.Blocked.Should().BeTrue();
    }
    
    [Fact]
    public void FilterOrder_ShouldBeSetCorrectly() {
        var sut = new VoidPredicateEventFilter(3, predicate);
        sut.FilterOrder.Should().Be(3);
    }
    
    [Fact]
    public void FilterOrder_DefaultsToZero() {
        var sut = new VoidPredicateEventFilter(predicate);
        sut.FilterOrder.Should().Be(0);
    }
}