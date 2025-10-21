using DisposableEvents;

namespace DisposableEvents.Tests.Events;

[TestSubject(typeof(FilteredEventHandler<>))]
public class FilteredEventHandlerTest {

    [Fact]
    public void InvokesAction_WhenFilterPasses() {
        // Arrange
        var filter = Substitute.For<IEventFilter<int>>();
        var handler = Substitute.For<IEventHandler<int>>();
        filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        var sut = new FilteredEventHandler<int>(handler, filter);

        // Act
        sut.Handle(10);

        // Assert
        handler.Received(1).Handle(10);
    }

    [Fact]
    public void DoesNotInvokeAction_WhenFilterBlocks() {
        // Arrange
        var filter = Substitute.For<IEventFilter<int>>();
        var handler = Substitute.For<IEventHandler<int>>();
        filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);
        var sut = new FilteredEventHandler<int>(handler, filter);

        // Act
        sut.Handle(20);

        // Assert
        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void PassesMessage_ToFilter() {
        // Arrange
        var filter = Substitute.For<IEventFilter<int>>();
        var handler = Substitute.For<IEventHandler<int>>();
        filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        var sut = new FilteredEventHandler<int>(handler, filter);

        // Act
        sut.Handle(30);
        
        // Assert
        filter.Received(1).Filter(ref Arg.Is<int>(m => m == 30));
    }
}