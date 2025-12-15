using DisposableEvents;

namespace DisposableEvents.Tests.Events.EventTypes;

[TestSubject(typeof(DisposableEvent))]
public class VoidDisposableEventTest {
    readonly DisposableEvent sut = new();
    readonly IEventHandler<Void>[] handlers = Enumerable.Range(0, 5).Select(_ => Substitute.For<IEventHandler<Void>>()).ToArray();

    [Fact]
    public void Publish_SendsMessageToHandlers() {
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }
        
        sut.Publish();
        
        Assert.All(handlers, h => h.Received(1).Handle(Void.Value));
    }

    [Fact]
    public void DisposedSubscription_Handler_DoesNotReceiveMessages() {
        var handler = Substitute.For<IEventHandler<Void>>();
        var subscription = sut.Subscribe(handler);

        subscription.Dispose();
        sut.Publish();

        handler.DidNotReceive().Handle(Arg.Any<Void>());
    }

    [Fact]
    public void Publish_AfterClearSubscriptions_DoesNotSendMessageToHandlers() {
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }

        sut.ClearHandlers();
        sut.Publish();

        Assert.All(handlers, h => h.DidNotReceive().Handle(Arg.Any<Void>()));
    }

    [Fact]
    public void Publish_AfterDispose_DoesNotSendMessageToHandlers() {
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }

        sut.Dispose();
        sut.Publish();

        Assert.All(handlers, h => h.DidNotReceive().Handle(Arg.Any<Void>()));
    }
    
    [Fact]
    public void Subscribe_AfterDispose_HandlerDoesNotReceiveMessages() {
        var handler = Substitute.For<IEventHandler<Void>>();
        sut.Dispose();

        sut.Subscribe(handler);
        sut.Publish();

        handler.DidNotReceive().Handle(Arg.Any<Void>());
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

        sut.ClearHandlers();

        sut.HandlerCount.Should().Be(0);
    }
    
    [Fact]
    public void IsDisposed_ReflectsDisposedState() {
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

    [Fact]
    public void HandlerCount_IncrementsWithEachSubscription() {
        sut.HandlerCount.Should().Be(0);
        
        var handler1 = Substitute.For<IEventHandler<Void>>();
        sut.Subscribe(handler1);
        sut.HandlerCount.Should().Be(1);
        
        var handler2 = Substitute.For<IEventHandler<Void>>();
        sut.Subscribe(handler2);
        sut.HandlerCount.Should().Be(2);
        
        var handler3 = Substitute.For<IEventHandler<Void>>();
        sut.Subscribe(handler3);
        sut.HandlerCount.Should().Be(3);
    }

    [Fact]
    public void HandlerCount_DecrementsWhenSubscriptionDisposed() {
        var handler1 = Substitute.For<IEventHandler<Void>>();
        var handler2 = Substitute.For<IEventHandler<Void>>();
        var handler3 = Substitute.For<IEventHandler<Void>>();
        
        var sub1 = sut.Subscribe(handler1);
        var sub2 = sut.Subscribe(handler2);
        var sub3 = sut.Subscribe(handler3);

        sut.HandlerCount.Should().Be(3);

        sub2.Dispose();
        sut.HandlerCount.Should().Be(2);

        sub1.Dispose();
        sut.HandlerCount.Should().Be(1);

        sub3.Dispose();
        sut.HandlerCount.Should().Be(0);
    }
}
