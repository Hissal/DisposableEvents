using DisposableEvents;

namespace DisposableEvents.Tests.Events.EventTypes;

[TestSubject(typeof(DisposableEvent))]
public class VoidDisposableEventTest {
    readonly DisposableEvent sut = new();

    [Fact]
    public void Publish_SendsMessageToHandlers() {
        var invocationCount = 0;
        sut.Subscribe(() => invocationCount++);
        sut.Subscribe(() => invocationCount++);
        sut.Subscribe(() => invocationCount++);
        
        sut.Publish();
        
        invocationCount.Should().Be(3);
    }

    [Fact]
    public void Publish_WithVoidValue_SendsMessageToHandlers() {
        var invocationCount = 0;
        sut.Subscribe(() => invocationCount++);
        sut.Subscribe(() => invocationCount++);
        
        sut.Publish(Void.Value);
        
        invocationCount.Should().Be(2);
    }

    [Fact]
    public void DisposedSubscription_Handler_DoesNotReceiveMessages() {
        var invocationCount = 0;
        var subscription = sut.Subscribe(() => invocationCount++);

        subscription.Dispose();
        sut.Publish();

        invocationCount.Should().Be(0);
    }

    [Fact]
    public void Publish_AfterClearSubscriptions_DoesNotSendMessageToHandlers() {
        var invocationCount = 0;
        sut.Subscribe(() => invocationCount++);
        sut.Subscribe(() => invocationCount++);
        sut.Subscribe(() => invocationCount++);

        sut.ClearHandlers();
        sut.Publish();

        invocationCount.Should().Be(0);
    }

    [Fact]
    public void Publish_AfterDispose_DoesNotSendMessageToHandlers() {
        var invocationCount = 0;
        sut.Subscribe(() => invocationCount++);
        sut.Subscribe(() => invocationCount++);
        sut.Subscribe(() => invocationCount++);

        sut.Dispose();
        sut.Publish();

        invocationCount.Should().Be(0);
    }
    
    [Fact]
    public void Subscribe_AfterDispose_HandlerDoesNotReceiveMessages() {
        var invocationCount = 0;
        sut.Dispose();

        sut.Subscribe(() => invocationCount++);
        sut.Publish();

        invocationCount.Should().Be(0);
    }
    
    [Fact]
    public void HandlerCount_ReturnsNumberOfSubscribedHandlers() {
        sut.Subscribe(() => { });
        sut.Subscribe(() => { });
        sut.Subscribe(() => { });

        sut.HandlerCount.Should().Be(3);
    }
    
    [Fact]
    public void HandlerCount_ShouldBeZero_AfterClearSubscriptions() {
        sut.Subscribe(() => { });
        sut.Subscribe(() => { });
        sut.Subscribe(() => { });

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
        var handler1 = Substitute.For<IEventHandler<Void>>();
        var handler2 = Substitute.For<IEventHandler<Void>>();
        var handler3 = Substitute.For<IEventHandler<Void>>();
        
        sut.Subscribe(handler1);
        sut.Subscribe(handler2);
        sut.Subscribe(handler3);

        using var handlerSnapshot = sut.SnapshotHandlers();
        var subscribedHandlers = handlerSnapshot.Span.ToArray();

        subscribedHandlers.Should().Contain(new[] { handler1, handler2, handler3 });
    }
    
    [Fact]
    public void GetHandlers_AfterDispose_ReturnsEmptyArray() {
        sut.Subscribe(() => { });
        sut.Subscribe(() => { });

        sut.Dispose();
        using var handlerSnapshot = sut.SnapshotHandlers();
        var subscribedHandlers = handlerSnapshot.Span.ToArray();

        subscribedHandlers.Should().BeEmpty();
    }

    [Fact]
    public void Subscribe_WithIEventHandler_ReceivesMessages() {
        var handler = Substitute.For<IEventHandler<Void>>();
        sut.Subscribe(handler);

        sut.Publish();

        handler.Received(1).Handle(Void.Value);
    }

    [Fact]
    public void Constructor_WithInitialCapacity_CreatesEvent() {
        var evt = new DisposableEvent(10);
        var invocationCount = 0;
        
        evt.Subscribe(() => invocationCount++);
        evt.Publish();

        invocationCount.Should().Be(1);
    }

    [Fact]
    public void MultipleSubscriptions_AllReceiveMessages() {
        var count1 = 0;
        var count2 = 0;
        var count3 = 0;

        sut.Subscribe(() => count1++);
        sut.Subscribe(() => count2++);
        sut.Subscribe(() => count3++);

        sut.Publish();

        count1.Should().Be(1);
        count2.Should().Be(1);
        count3.Should().Be(1);
    }

    [Fact]
    public void Subscribe_StatefulOverload_PassesStateToHandler() {
        var state = new { Value = 0 };
        var capturedState = (object?)null;

        sut.Subscribe(state, s => capturedState = s);
        sut.Publish();

        capturedState.Should().Be(state);
    }

    [Fact]
    public void HandlerCount_IncrementsWithEachSubscription() {
        sut.HandlerCount.Should().Be(0);
        
        sut.Subscribe(() => { });
        sut.HandlerCount.Should().Be(1);
        
        sut.Subscribe(() => { });
        sut.HandlerCount.Should().Be(2);
        
        sut.Subscribe(() => { });
        sut.HandlerCount.Should().Be(3);
    }

    [Fact]
    public void HandlerCount_DecrementsWhenSubscriptionDisposed() {
        var sub1 = sut.Subscribe(() => { });
        var sub2 = sut.Subscribe(() => { });
        var sub3 = sut.Subscribe(() => { });

        sut.HandlerCount.Should().Be(3);

        sub2.Dispose();
        sut.HandlerCount.Should().Be(2);

        sub1.Dispose();
        sut.HandlerCount.Should().Be(1);

        sub3.Dispose();
        sut.HandlerCount.Should().Be(0);
    }
}
