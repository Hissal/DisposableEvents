using DisposableEvents;

namespace DisposableEvents.Tests.Events.EventTypes;

[TestSubject(typeof(FilterAttachingEvent<>))]
public class FilterAttachingEventTest {
    sealed class PassingFilter : IEventFilter<int> {
        public int FilterOrder { get; } = 0;
        FilterResult IEventFilter<int>.Filter(ref int value) => FilterResult.Pass;
    }
    
    readonly FilterAttachingEvent<int> sut = new(new PassingFilter());
    readonly IEventHandler<int>[] handlers = Enumerable.Range(0, 5).Select(_ => Substitute.For<IEventHandler<int>>()).ToArray();
    const int c_message = 69;

    // ----- Filtering Tests ----- //

    [Fact]
    public void AttachesFilterToHandler() {
        var filter = Substitute.For<IEventFilter<int>>();
        var sutWithFilter = new FilterAttachingEvent<int>(filter);
        var handler = Substitute.For<IEventHandler<int>>();
        
        sutWithFilter.Subscribe(handler);
        
        var subscribedHandler = sutWithFilter.GetHandlers().First();
        subscribedHandler.Should().BeOfType<FilteredEventHandler<int>>();
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

        var subscribedHandlers = sut.GetHandlers();

        // Extract the original handlers from the FilteredEventHandler wrappers
        subscribedHandlers.Select(f => f.GetAnyPrivateFieldOrPropertyOfType<IEventHandler<int>>()).Should().BeEquivalentTo(handlers);
    }

    [Fact]
    public void GetHandlers_AfterDispose_ReturnsEmptyArray() {
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }

        sut.Dispose();
        var subscribedHandlers = sut.GetHandlers();

        subscribedHandlers.Should().BeEmpty();
    }
}
