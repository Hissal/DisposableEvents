using DisposableEvents;

namespace DisposableEvents.Tests.Events.EventTypes;

[TestSubject(typeof(FilteredEvent<>))]
public class FilteredEventTest {
    sealed class PassingFilter : IEventFilter<int> {
        public int FilterOrder { get; } = 0;
        FilterResult IEventFilter<int>.Filter(ref int value) => FilterResult.Pass;
    }
    
    readonly FilteredEvent<int> sut = new(new PassingFilter());
    readonly IEventHandler<int>[] handlers = Enumerable.Range(0, 5).Select(_ => Substitute.For<IEventHandler<int>>()).ToArray();
    const int c_message = 69;

    // ----- Filtering Tests ----- //
    
    [Fact]
    public void Filters_AreApplied_BeforeSendingMessageToHandlers() {
        var filter = Substitute.For<IEventFilter<int>>();

        var msg = c_message;
        filter.Filter(ref msg).Returns(FilterResult.Block);

        var sutWithFilter = new FilteredEvent<int>(filter);

        foreach (var handler in handlers) {
            sutWithFilter.Subscribe(handler);
        }

        sutWithFilter.Publish(c_message);

        Assert.All(handlers, h => h.DidNotReceive().Handle(Arg.Any<int>()));
        filter.Received(1).Filter(ref msg);
    }
    
    // ----- General Tests ----- //

    [Fact]
    public void Publish_SendsMessageToHandlers() {
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }

        sut.Publish(c_message);

        Assert.All(handlers, h => h.Received(1).Handle(c_message));
    }

    [Fact]
    public void DisposedSubscription_Handler_DoesNotReceiveMessages() {
        var handler = Substitute.For<IEventHandler<int>>();
        var subscription = sut.Subscribe(handler);

        subscription.Dispose();
        sut.Publish(c_message);

        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void Publish_AfterClearSubscriptions_DoesNotSendMessageToHandlers() {
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }

        sut.ClearSubscriptions();
        sut.Publish(c_message);

        Assert.All(handlers, h => h.DidNotReceive().Handle(Arg.Any<int>()));
    }

    [Fact]
    public void Publish_AfterDispose_DoesNotSendMessageToHandlers() {
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }

        sut.Dispose();
        sut.Publish(c_message);

        Assert.All(handlers, h => h.DidNotReceive().Handle(Arg.Any<int>()));
    }

    [Fact]
    public void Subscribe_AfterDispose_HandlerDoesNotReceiveMessages() {
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Dispose();

        sut.Subscribe(handler);
        sut.Publish(c_message);

        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void HandlerCount_ReturnsNumberOfSubscribedHandlers() {
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }

        sut.HandlerCount.Should().Be(handlers.Length);
    }

    [Fact]
    public void HandlerCount_ShouldBeZero_AfterClearSubscriptions() {
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }

        sut.ClearSubscriptions();

        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void IsDisposed_ReturnsReflectsDisposedState() {
        sut.IsDisposed.Should().BeFalse();
        sut.Dispose();
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void GetHandlers_ReturnsSubscribedHandlers() {
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }

        using var handlerSnapshot = sut.SnapshotHandlers();
        var subscribedHandlers = handlerSnapshot.Span.ToArray();

        subscribedHandlers.Should().Contain(handlers);
    }

    [Fact]
    public void GetHandlers_AfterDispose_ReturnsEmptyArray() {
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }

        sut.Dispose();
        using var handlerSnapshot = sut.SnapshotHandlers();
        var subscribedHandlers = handlerSnapshot.Span.ToArray();

        subscribedHandlers.Should().BeEmpty();
    }
}
