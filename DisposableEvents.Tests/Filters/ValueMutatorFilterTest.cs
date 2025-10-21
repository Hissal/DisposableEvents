using DisposableEvents;

namespace DisposableEvents.Tests.Filters;

[TestSubject(typeof(ValueMutatorFilter<>))]
public class ValueMutatorFilterTest {
    readonly Func<int, int> mutator = Substitute.For<Func<int, int>>();
    int message = 5;
    
    ValueMutatorFilter<int> Sut => new(mutator);

    [Fact]
    public void Mutator_ShouldReceiveMessage() {
        var original = message;
        Sut.Filter(ref message);
        mutator.Received(1).Invoke(original);
    }
 
    [Fact]
    public void Mutator_ShouldMutateValue() {
        mutator.Invoke(Arg.Any<int>()).Returns(34);
        Sut.Filter(ref message);
        message.Should().Be(34);
    }

    [Fact]
    public void Filter_ShouldReturnPass() {
        var result = Sut.Filter(ref message);
        result.Passed.Should().BeTrue();
    }
    
    [Fact]
    public void FilterOrder_GetsSetCorrectly() {
        var filter = new ValueMutatorFilter<int>(42, mutator);
        filter.FilterOrder.Should().Be(42);
    }
    
    [Fact]
    public void FilterOrder_DefaultsToZero() {
        var filter = new ValueMutatorFilter<int>(mutator);
        filter.FilterOrder.Should().Be(0);
    }
}