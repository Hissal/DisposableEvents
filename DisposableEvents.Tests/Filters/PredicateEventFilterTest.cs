using DisposableEvents;

namespace DisposableEvents.Tests.Filters;

[TestSubject(typeof(PredicateEventFilter<>))]
public class PredicateEventFilterTest {
    readonly Func<int, bool> predicate = Substitute.For<Func<int, bool>>();
    int message = 69;

    PredicateEventFilter<int> Sut => new(predicate);

    [Fact]
    public void Should_ReturnFilterResultPassed_WhenPredicateTrue() {
        predicate.Invoke(Arg.Any<int>()).Returns(true);
        var result = Sut.Filter(ref message);
        result.Passed.Should().BeTrue();
    }
    
    [Fact]
    public void Should_ReturnFilterResultBlocked_WhenPredicateFalse() {
        predicate.Invoke(Arg.Any<int>()).Returns(false);
        var result = Sut.Filter(ref message);
        result.Blocked.Should().BeTrue();
    }
    
    [Fact]
    public void FilterOrder_ShouldBeSetCorrectly() {
        var sut = new PredicateEventFilter<int>(3, predicate);
        sut.FilterOrder.Should().Be(3);
    }

    [Fact]
    public void Predicate_ShouldReceiveMessage() {
        Sut.Filter(ref message);
        predicate.Received(1).Invoke(message);
    }
    
    [Fact]
    public void FilterOrder_DefaultsToZero() {
        var sut = new PredicateEventFilter<int>(predicate);
        sut.FilterOrder.Should().Be(0);
    }
}