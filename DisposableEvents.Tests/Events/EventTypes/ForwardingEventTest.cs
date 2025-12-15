namespace DisposableEvents.Tests.Events.EventTypes;

[TestSubject(typeof(ForwardingEvent<>))]
public class ForwardingEventTest {
    const int c_message = 42;

    // ----- Constructor Tests ----- //

    [Fact]
    public void Constructor_WithSingleTarget_CreatesForwardingEvent() {
        var target = new DisposableEvent<int>();
        var sut = new ForwardingEvent<int>(target);

        sut.Should().NotBeNull();
        sut.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithMultipleTargets_CreatesForwardingEvent() {
        var targets = new[] { new DisposableEvent<int>(), new DisposableEvent<int>() };
        var sut = new ForwardingEvent<int>(targets);

        sut.Should().NotBeNull();
        sut.IsDisposed.Should().BeFalse();
    }

    // ----- Publish Flag Tests ----- //

    [Fact]
    public void Publish_WithPublishFlag_ForwardsMessageToTarget() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        target.Subscribe(handler);

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Publish);
        sut.Publish(c_message);

        handler.Received(1).Handle(c_message);
    }

    [Fact]
    public void Publish_WithoutPublishFlag_DoesNotForwardMessage() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        target.Subscribe(handler);

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);
        sut.Publish(c_message);

        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void Publish_WithIncludeSelfFlag_SendsMessageToOwnHandlers() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.IncludeSelf);
        sut.Subscribe(handler);
        sut.Publish(c_message);

        handler.Received(1).Handle(c_message);
    }

    [Fact]
    public void Publish_WithoutIncludeSelfFlag_DoesNotSendMessageToOwnHandlers() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Publish);
        sut.Subscribe(handler);
        sut.Publish(c_message);

        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void Publish_ToMultipleTargets_ForwardsToAllTargets() {
        var target1 = new DisposableEvent<int>();
        var target2 = new DisposableEvent<int>();
        var handler1 = Substitute.For<IEventHandler<int>>();
        var handler2 = Substitute.For<IEventHandler<int>>();
        target1.Subscribe(handler1);
        target2.Subscribe(handler2);

        var sut = new ForwardingEvent<int>(new[] { target1, target2 }, ForwardTiming.AfterSelf, ForwardFlags.Publish);
        sut.Publish(c_message);

        handler1.Received(1).Handle(c_message);
        handler2.Received(1).Handle(c_message);
    }

    // ----- Subscribe Flag Tests ----- //

    [Fact]
    public void Subscribe_WithSubscribeFlag_ForwardsSubscriptionToTarget() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);
        sut.Subscribe(handler);
        target.Publish(c_message);

        handler.Received(1).Handle(c_message);
    }

    [Fact]
    public void Subscribe_WithoutSubscribeFlag_DoesNotForwardSubscription() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Publish);
        sut.Subscribe(handler);
        target.Publish(c_message);

        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void Subscribe_ToMultipleTargets_ForwardsToAllTargets() {
        var target1 = new DisposableEvent<int>();
        var target2 = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(new[] { target1, target2 }, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);
        sut.Subscribe(handler);

        target1.Publish(c_message);
        target2.Publish(c_message);

        handler.Received(2).Handle(c_message);
    }

    [Fact]
    public void Subscribe_ReturnsDisposable_ThatUnsubscribesFromTargets() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);
        var subscription = sut.Subscribe(handler);

        subscription.Dispose();
        target.Publish(c_message);

        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    // ----- Dispose Flag Tests ----- //

    [Fact]
    public void Dispose_WithDisposeFlag_DisposesTargets() {
        var target = new DisposableEvent<int>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Dispose);
        sut.Dispose();

        target.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_WithoutDisposeFlag_DoesNotDisposeTargets() {
        var target = new DisposableEvent<int>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);
        sut.Dispose();

        target.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Dispose_WithIncludeSelfFlag_DisposesOwnCore() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.IncludeSelf);
        sut.Subscribe(handler);
        sut.Dispose();
        sut.Publish(c_message);

        handler.DidNotReceive().Handle(Arg.Any<int>());
        sut.IsDisposed.Should().BeTrue();
    }

    // ----- ClearSubscriptions Flag Tests ----- //

    [Fact]
    public void ClearHandlers_WithClearSubscriptionsFlag_ClearsTargetHandlers() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        target.Subscribe(handler);

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.ClearSubscriptions);
        sut.ClearHandlers();
        target.Publish(c_message);

        handler.DidNotReceive().Handle(Arg.Any<int>());
        target.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void ClearHandlers_WithoutClearSubscriptionsFlag_DoesNotClearTargetHandlers() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        target.Subscribe(handler);

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);
        sut.ClearHandlers();
        target.Publish(c_message);

        handler.Received(1).Handle(c_message);
        target.HandlerCount.Should().Be(1);
    }

    [Fact]
    public void ClearHandlers_WithIncludeSelfFlag_ClearsOwnHandlers() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.IncludeSelf);
        sut.Subscribe(handler);
        sut.ClearHandlers();
        sut.Publish(c_message);

        handler.DidNotReceive().Handle(Arg.Any<int>());
        sut.HandlerCount.Should().Be(0);
    }

    // ----- ForwardTiming Tests ----- //

    [Fact]
    public void Publish_WithAfterSelfTiming_PublishesInCorrectOrder() {
        var receivedMessages = new List<string>();
        var target = new DisposableEvent<int>();
        target.Subscribe(new EventHandler<int>(_ => receivedMessages.Add("target")));

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Publish | ForwardFlags.IncludeSelf);
        sut.Subscribe(new EventHandler<int>(_ => receivedMessages.Add("self")));

        sut.Publish(c_message);

        receivedMessages.Should().Equal("self", "target");
    }

    [Fact]
    public void Publish_WithBeforeSelfTiming_PublishesInCorrectOrder() {
        var receivedMessages = new List<string>();
        var target = new DisposableEvent<int>();
        target.Subscribe(new EventHandler<int>(_ => receivedMessages.Add("target")));

        var sut = new ForwardingEvent<int>(target, ForwardTiming.BeforeSelf, ForwardFlags.Publish | ForwardFlags.IncludeSelf);
        sut.Subscribe(new EventHandler<int>(_ => receivedMessages.Add("self")));

        sut.Publish(c_message);

        receivedMessages.Should().Equal("target", "self");
    }

    [Fact]
    public void Subscribe_WithAfterSelfTiming_SubscribesInCorrectOrder() {
        var callOrder = new List<string>();
        var target = new DisposableEvent<int>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe | ForwardFlags.IncludeSelf);

        // With AfterSelf timing, when subscribing, it should:
        // 1. Subscribe to self's core first
        // 2. Then subscribe to target
        var handler = new EventHandler<int>(_ => callOrder.Add("handler"));
        sut.Subscribe(handler);

        // When target publishes, the handler (subscribed to target) should be called
        target.Publish(c_message);

        callOrder.Should().Contain("handler");
        callOrder.Count.Should().Be(1);
    }

    [Fact]
    public void Subscribe_WithBeforeSelfTiming_SubscribesInCorrectOrder() {
        var callOrder = new List<string>();
        var target = new DisposableEvent<int>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.BeforeSelf, ForwardFlags.Subscribe | ForwardFlags.IncludeSelf);

        // With BeforeSelf timing, when subscribing, it should:
        // 1. Subscribe to target first
        // 2. Then subscribe to self's core
        var handler = new EventHandler<int>(_ => callOrder.Add("handler"));
        sut.Subscribe(handler);

        // When target publishes, the handler (subscribed to target) should be called
        target.Publish(c_message);

        callOrder.Should().Contain("handler");
        callOrder.Count.Should().Be(1);
    }

    // ----- Combined Flags Tests ----- //

    [Fact]
    public void ForwardFlags_PubSub_CombinesPublishAndSubscribe() {
        var target = new DisposableEvent<int>();
        var handler1 = Substitute.For<IEventHandler<int>>();
        var handler2 = Substitute.For<IEventHandler<int>>();
        target.Subscribe(handler2);

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.PubSub);
        sut.Subscribe(handler1);
        sut.Publish(c_message);
        target.Publish(c_message * 2);

        // handler1 subscribed to sut, which forwards to target (Subscribe flag)
        // So handler1 receives both sut.Publish (forwarded) and target.Publish
        handler1.Received(1).Handle(c_message);
        handler1.Received(1).Handle(c_message * 2);
        
        // handler2 subscribed directly to target
        // Receives from sut.Publish (forwarded) and target.Publish
        handler2.Received(1).Handle(c_message);
        handler2.Received(1).Handle(c_message * 2);
    }

    [Fact]
    public void ForwardFlags_AllWithSelf_CombinesAllFlagsIncludingSelf() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.AllWithSelf);
        sut.Subscribe(handler);
        sut.Publish(c_message);

        // With AllWithSelf (Publish | Subscribe | IncludeSelf):
        // - Subscribe forwards the handler to target
        // - Publish forwards the message to target
        // - IncludeSelf means sut also has own handlers
        // So handler receives from both sut's own core AND from target
        handler.Received(2).Handle(c_message);
        sut.HandlerCount.Should().Be(1);
    }

    [Fact]
    public void ForwardFlags_AllWithoutSelf_CombinesAllFlagsExceptSelf() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.AllWithoutSelf);
        sut.Subscribe(handler);
        sut.Publish(c_message);

        // With AllWithoutSelf (Publish | Subscribe, no IncludeSelf):
        // - Subscribe forwards the handler to target
        // - Publish forwards the message to target
        // - No IncludeSelf means sut has no own handlers
        // Handler receives from target only
        handler.Received(1).Handle(c_message);
        sut.HandlerCount.Should().Be(0);
    }

    // ----- Standard Event Behavior Tests ----- //

    [Fact]
    public void IsDisposed_ReturnsFalse_WhenNotDisposed() {
        var target = new DisposableEvent<int>();
        var sut = new ForwardingEvent<int>(target);

        sut.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void IsDisposed_ReturnsTrue_AfterDispose() {
        var target = new DisposableEvent<int>();
        var sut = new ForwardingEvent<int>(target);

        sut.Dispose();

        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_IsIdempotent() {
        var target = new DisposableEvent<int>();
        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Dispose);

        sut.Dispose();
        sut.Dispose();

        target.IsDisposed.Should().BeTrue();
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void HandlerCount_ReturnsZero_WithoutIncludeSelf() {
        var target = new DisposableEvent<int>();
        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);

        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void HandlerCount_ReturnsCorrectCount_WithIncludeSelf() {
        var target = new DisposableEvent<int>();
        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.IncludeSelf);

        sut.Subscribe(Substitute.For<IEventHandler<int>>());
        sut.Subscribe(Substitute.For<IEventHandler<int>>());

        sut.HandlerCount.Should().Be(2);
    }

    [Fact]
    public void Publish_AfterDispose_DoesNotPublish() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        target.Subscribe(handler);

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Publish);
        sut.Dispose();
        sut.Publish(c_message);

        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void Subscribe_AfterDispose_ReturnsEmptyDisposable() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe | ForwardFlags.IncludeSelf);
        sut.Dispose();
        var subscription = sut.Subscribe(handler);

        subscription.Should().NotBeNull();
        sut.Publish(c_message);
        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void ClearHandlers_AfterDispose_DoesNotThrow() {
        var target = new DisposableEvent<int>();
        var sut = new ForwardingEvent<int>(target);

        sut.Dispose();

        var act = () => sut.ClearHandlers();
        act.Should().NotThrow();
    }

    [Fact]
    public void SnapshotHandlers_WithoutIncludeSelf_ReturnsEmpty() {
        var target = new DisposableEvent<int>();
        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);

        using var snapshot = sut.SnapshotHandlers();

        snapshot.Span.Length.Should().Be(0);
    }

    [Fact]
    public void SnapshotHandlers_WithIncludeSelf_ReturnsHandlers() {
        var target = new DisposableEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.IncludeSelf);
        sut.Subscribe(handler);

        using var snapshot = sut.SnapshotHandlers();

        snapshot.Span.Length.Should().Be(1);
        snapshot.Span[0].Should().Be(handler);
    }
}

[TestSubject(typeof(ForwardingEventHandler<>))]
public class ForwardingEventHandlerTest {
    const int c_message = 42;

    [Fact]
    public void Handle_WithSingleTarget_ForwardsMessage() {
        var target = Substitute.For<IEventPublisher<int>>();
        var handler = new ForwardingEventHandler<int>(target);

        handler.Handle(c_message);

        target.Received(1).Publish(c_message);
    }

    [Fact]
    public void Handle_WithMultipleTargets_ForwardsToAllTargets() {
        var target1 = Substitute.For<IEventPublisher<int>>();
        var target2 = Substitute.For<IEventPublisher<int>>();
        var handler = new ForwardingEventHandler<int>(target1, target2);

        handler.Handle(c_message);

        target1.Received(1).Publish(c_message);
        target2.Received(1).Publish(c_message);
    }
}

[TestSubject(typeof(ForwardingSubscriptionExtensions))]
public class ForwardingSubscriptionExtensionsTest {
    const int c_message = 42;

    [Fact]
    public void ForwardTo_WithSinglePublisher_ForwardsMessages() {
        var source = new DisposableEvent<int>();
        var target = Substitute.For<IEventPublisher<int>>();

        source.ForwardTo(target);
        source.Publish(c_message);

        target.Received(1).Publish(c_message);
    }

    [Fact]
    public void ForwardTo_WithMultiplePublishers_ForwardsToAll() {
        var source = new DisposableEvent<int>();
        var target1 = Substitute.For<IEventPublisher<int>>();
        var target2 = Substitute.For<IEventPublisher<int>>();

        source.ForwardTo(target1, target2);
        source.Publish(c_message);

        target1.Received(1).Publish(c_message);
        target2.Received(1).Publish(c_message);
    }

    [Fact]
    public void ForwardTo_ReturnsDisposable_ThatStopsForwarding() {
        var source = new DisposableEvent<int>();
        var target = Substitute.For<IEventPublisher<int>>();

        var subscription = source.ForwardTo(target);
        subscription.Dispose();
        source.Publish(c_message);

        target.DidNotReceive().Publish(Arg.Any<int>());
    }
}
