using DisposableEvents;

namespace DisposableEvents.Tests.Filters;

[TestSubject(typeof(VoidPredicateEventFilter<>))]
public class StatefulVoidPredicateEventFilterTest {
    readonly Func<int, bool> predicate = Substitute.For<Func<int, bool>>();
    Void message = Void.Value;
    const int c_state = 7;
    
    VoidPredicateEventFilter<int> Sut => new(c_state, predicate);

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
        var sut = new VoidPredicateEventFilter<int>(c_state, 3, predicate);
        sut.FilterOrder.Should().Be(3);
    }
    
    [Fact]
    public void Predicate_ShouldBeCalledWithState() {
        Sut.Filter(ref message);
        predicate.Received(1).Invoke(c_state);
    }
    
    [Fact]
    public void FilterOrder_DefaultsToZero() {
        var sut = new VoidPredicateEventFilter<int>(c_state, predicate);
        sut.FilterOrder.Should().Be(0);
    }
}