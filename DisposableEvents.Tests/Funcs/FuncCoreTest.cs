namespace DisposableEvents.Tests.Funcs;

[TestSubject(typeof(FuncCore<,>))]
public class FuncCoreTest {
    readonly FuncCore<int, string> sut = new();

    [Fact]
    public void SubscriptionDispose_AfterClearHandlers_DoesNotThrow() {
        // Arrange
        var handler = Substitute.For<IFuncHandler<int, string>>();
        var subscription = sut.AddHandler(handler);
        sut.ClearHandlers();

        // Act
        var act = () => subscription.Dispose();

        // Assert
        act.Should().NotThrow("a subscription invalidated by ClearHandlers should dispose as a no-op");
    }

    [Fact]
    public void SubscriptionDispose_AfterClearHandlers_DoesNotRemoveHandlerReusingTheKey() {
        // Arrange
        var staleHandler = Substitute.For<IFuncHandler<int, string>>();
        var staleSubscription = sut.AddHandler(staleHandler);

        sut.ClearHandlers();

        var newHandler = Substitute.For<IFuncHandler<int, string>>();
        sut.AddHandler(newHandler); // Reuses the key freed by ClearHandlers.

        // Act
        staleSubscription.Dispose();
        sut.Invoke(42);

        // Assert
        newHandler.Received(1).Handle(42);
    }

    [Fact]
    public void SubscriptionDispose_AfterClearHandlers_LeavesHandlerCountUnchanged() {
        // Arrange
        var staleSubscription = sut.AddHandler(Substitute.For<IFuncHandler<int, string>>());
        sut.ClearHandlers();
        sut.AddHandler(Substitute.For<IFuncHandler<int, string>>());

        // Act
        staleSubscription.Dispose();

        // Assert
        sut.HandlerCount.Should().Be(1);
    }
}
