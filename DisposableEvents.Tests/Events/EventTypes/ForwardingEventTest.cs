using DisposableEvents;
using DisposableEvents.Disposables;

namespace DisposableEvents.Tests.Events.EventTypes;

[TestSubject(typeof(ForwardingEvent<>))]
public class ForwardingEventTest {
    const int c_message = 42;

    /// <summary>
    /// Creates a functional substitute that behaves like a real IDisposableEvent.
    /// The substitute maintains a list of handlers and properly implements Subscribe/Publish/ClearHandlers.
    /// Use this when tests need actual event behavior (e.g., testing integration of multiple flags).
    /// For simple forwarding tests, use Substitute.For directly with When/Do for specific method calls.
    /// </summary>
    static IDisposableEvent<T> CreateFunctionalSubstitute<T>() {
        var substitute = Substitute.For<IDisposableEvent<T>>();
        var handlers = new List<IEventHandler<T>>();
        
        substitute.Subscribe(Arg.Any<IEventHandler<T>>()).Returns(callInfo => {
            var handler = callInfo.Arg<IEventHandler<T>>();
            handlers.Add(handler);
            return Disposable.Action(() => handlers.Remove(handler));
        });
        
        substitute.When(x => x.Publish(Arg.Any<T>()))
            .Do(callInfo => {
                var message = callInfo.Arg<T>();
                // Iterate over a copy to avoid modification during iteration
                foreach (var handler in handlers.ToArray()) {
                    handler.Handle(message);
                }
            });
        
        substitute.When(x => x.ClearHandlers())
            .Do(_ => handlers.Clear());
        
        substitute.IsDisposed.Returns(false);
        substitute.HandlerCount.Returns(_ => handlers.Count);
        
        return substitute;
    }

    // ----- Constructor Tests ----- //

    [Fact]
    public void Constructor_WithSingleTarget_CreatesForwardingEvent() {
        var target = Substitute.For<IDisposableEvent<int>>();
        var sut = new ForwardingEvent<int>(target);

        sut.Should().NotBeNull();
        sut.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithMultipleTargets_CreatesForwardingEvent() {
        var targets = new[] { Substitute.For<IDisposableEvent<int>>(), Substitute.For<IDisposableEvent<int>>() };
        var sut = new ForwardingEvent<int>(targets);

        sut.Should().NotBeNull();
        sut.IsDisposed.Should().BeFalse();
    }

    // ----- Publish Flag Tests ----- //

    [Fact]
    public void Publish_WithPublishFlag_ForwardsMessageToTarget() {
        var target = Substitute.For<IDisposableEvent<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Publish);
        sut.Publish(c_message);

        target.Received(1).Publish(c_message);
    }

    [Fact]
    public void Publish_WithoutPublishFlag_DoesNotForwardMessage() {
        var target = Substitute.For<IDisposableEvent<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);
        sut.Publish(c_message);

        target.DidNotReceive().Publish(Arg.Any<int>());
    }

    [Fact]
    public void Publish_WithIncludeSelfFlag_SendsMessageToOwnHandlers() {
        var target = Substitute.For<IDisposableEvent<int>>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.IncludeSelf);
        sut.Subscribe(handler);
        sut.Publish(c_message);

        handler.Received(1).Handle(c_message);
    }

    [Fact]
    public void Publish_WithoutIncludeSelfFlag_DoesNotSendMessageToOwnHandlers() {
        var target = Substitute.For<IDisposableEvent<int>>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Publish);
        sut.Subscribe(handler);
        sut.Publish(c_message);

        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void Publish_ToMultipleTargets_ForwardsToAllTargets() {
        var target1 = Substitute.For<IDisposableEvent<int>>();
        var target2 = Substitute.For<IDisposableEvent<int>>();

        var sut = new ForwardingEvent<int>(new[] { target1, target2 }, ForwardTiming.AfterSelf, ForwardFlags.Publish);
        sut.Publish(c_message);

        target1.Received(1).Publish(c_message);
        target2.Received(1).Publish(c_message);
    }

    // ----- Subscribe Flag Tests ----- //

    [Fact]
    public void Subscribe_WithSubscribeFlag_ForwardsSubscriptionToTarget() {
        var target = Substitute.For<IDisposableEvent<int>>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);
        sut.Subscribe(handler);

        target.Received(1).Subscribe(Arg.Any<IEventHandler<int>>());
    }

    [Fact]
    public void Subscribe_WithoutSubscribeFlag_DoesNotForwardSubscription() {
        var target = Substitute.For<IDisposableEvent<int>>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Publish);
        sut.Subscribe(handler);

        target.DidNotReceive().Subscribe(Arg.Any<IEventHandler<int>>());
    }

    [Fact]
    public void Subscribe_ToMultipleTargets_ForwardsToAllTargets() {
        var target1 = Substitute.For<IDisposableEvent<int>>();
        var target2 = Substitute.For<IDisposableEvent<int>>();
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(new[] { target1, target2 }, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);
        sut.Subscribe(handler);

        target1.Received(1).Subscribe(Arg.Any<IEventHandler<int>>());
        target2.Received(1).Subscribe(Arg.Any<IEventHandler<int>>());
    }

    [Fact]
    public void Subscribe_ReturnsDisposable_ThatUnsubscribesFromTargets() {
        var target = Substitute.For<IDisposableEvent<int>>();
        var disposable = Substitute.For<IDisposable>();
        target.Subscribe(Arg.Any<IEventHandler<int>>()).Returns(disposable);
        var handler = Substitute.For<IEventHandler<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);
        var subscription = sut.Subscribe(handler);

        subscription.Dispose();

        disposable.Received(1).Dispose();
    }

    // ----- Dispose Flag Tests ----- //

    [Fact]
    public void Dispose_WithDisposeFlag_DisposesTargets() {
        var target = Substitute.For<IDisposableEvent<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Dispose);
        sut.Dispose();

        target.Received(1).Dispose();
    }

    [Fact]
    public void Dispose_WithoutDisposeFlag_DoesNotDisposeTargets() {
        var target = Substitute.For<IDisposableEvent<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);
        sut.Dispose();

        target.DidNotReceive().Dispose();
    }

    [Fact]
    public void Dispose_WithIncludeSelfFlag_DisposesOwnCore() {
        var target = Substitute.For<IDisposableEvent<int>>();
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
        var target = Substitute.For<IDisposableEvent<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.ClearSubscriptions);
        sut.ClearHandlers();

        target.Received(1).ClearHandlers();
    }

    [Fact]
    public void ClearHandlers_WithoutClearSubscriptionsFlag_DoesNotClearTargetHandlers() {
        var target = Substitute.For<IDisposableEvent<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);
        sut.ClearHandlers();

        target.DidNotReceive().ClearHandlers();
    }

    [Fact]
    public void ClearHandlers_WithIncludeSelfFlag_ClearsOwnHandlers() {
        var target = Substitute.For<IDisposableEvent<int>>();
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
        var target = Substitute.For<IDisposableEvent<int>>();
        target.When(t => t.Publish(Arg.Any<int>())).Do(_ => receivedMessages.Add("target"));

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Publish | ForwardFlags.IncludeSelf);
        var handler = Substitute.For<IEventHandler<int>>();
        handler.When(h => h.Handle(Arg.Any<int>())).Do(_ => receivedMessages.Add("self"));
        sut.Subscribe(handler);

        sut.Publish(c_message);

        receivedMessages.Should().Equal("self", "target");
    }

    [Fact]
    public void Publish_WithBeforeSelfTiming_PublishesInCorrectOrder() {
        var receivedMessages = new List<string>();
        var target = Substitute.For<IDisposableEvent<int>>();
        target.When(t => t.Publish(Arg.Any<int>())).Do(_ => receivedMessages.Add("target"));

        var sut = new ForwardingEvent<int>(target, ForwardTiming.BeforeSelf, ForwardFlags.Publish | ForwardFlags.IncludeSelf);
        var handler = Substitute.For<IEventHandler<int>>();
        handler.When(h => h.Handle(Arg.Any<int>())).Do(_ => receivedMessages.Add("self"));
        sut.Subscribe(handler);

        sut.Publish(c_message);

        receivedMessages.Should().Equal("target", "self");
    }

    [Fact]
    public void Subscribe_WithAfterSelfTiming_SubscribesInCorrectOrder() {
        var subscribeOrder = new List<string>();
        var target = Substitute.For<IDisposableEvent<int>>();
        
        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe | ForwardFlags.IncludeSelf);

        target.When(t => t.Subscribe(Arg.Any<IEventHandler<int>>())).Do(_ => {
            // With AfterSelf timing, self's handler count should be 1 when target.Subscribe is called
            subscribeOrder.Add($"target (sut.HandlerCount={sut.HandlerCount})");
        });

        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);

        // Target should have been subscribed to after self, so HandlerCount should be 1
        subscribeOrder.Should().Equal("target (sut.HandlerCount=1)");
        target.Received(1).Subscribe(Arg.Any<IEventHandler<int>>());
    }

    [Fact]
    public void Subscribe_WithBeforeSelfTiming_SubscribesInCorrectOrder() {
        var subscribeOrder = new List<string>();
        var target = Substitute.For<IDisposableEvent<int>>();
        
        var sut = new ForwardingEvent<int>(target, ForwardTiming.BeforeSelf, ForwardFlags.Subscribe | ForwardFlags.IncludeSelf);

        target.When(t => t.Subscribe(Arg.Any<IEventHandler<int>>())).Do(_ => {
            // With BeforeSelf timing, self's handler count should be 0 when target.Subscribe is called
            subscribeOrder.Add($"target (sut.HandlerCount={sut.HandlerCount})");
        });

        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);

        // Target should have been subscribed to before self, so HandlerCount should be 0
        subscribeOrder.Should().Equal("target (sut.HandlerCount=0)");
        target.Received(1).Subscribe(Arg.Any<IEventHandler<int>>());
    }

    // ----- Combined Flags Tests ----- //

    [Fact]
    public void ForwardFlags_PubSub_CombinesPublishAndSubscribe() {
        var target = CreateFunctionalSubstitute<int>();
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
        var target = CreateFunctionalSubstitute<int>();
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
        var target = CreateFunctionalSubstitute<int>();
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
        var target = Substitute.For<IDisposableEvent<int>>();
        var sut = new ForwardingEvent<int>(target);

        sut.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void IsDisposed_ReturnsTrue_AfterDispose() {
        var target = Substitute.For<IDisposableEvent<int>>();
        var sut = new ForwardingEvent<int>(target);

        sut.Dispose();

        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_IsIdempotent() {
        var target = Substitute.For<IDisposableEvent<int>>();
        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Dispose);

        sut.Dispose();
        sut.Dispose();

        target.Received(1).Dispose();
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void HandlerCount_ReturnsZero_WithoutIncludeSelf() {
        var target = Substitute.For<IDisposableEvent<int>>();
        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);

        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void HandlerCount_ReturnsCorrectCount_WithIncludeSelf() {
        var target = Substitute.For<IDisposableEvent<int>>();
        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.IncludeSelf);

        sut.Subscribe(Substitute.For<IEventHandler<int>>());
        sut.Subscribe(Substitute.For<IEventHandler<int>>());

        sut.HandlerCount.Should().Be(2);
    }

    [Fact]
    public void Publish_AfterDispose_DoesNotPublish() {
        var target = Substitute.For<IDisposableEvent<int>>();

        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Publish);
        sut.Dispose();
        sut.Publish(c_message);

        target.DidNotReceive().Publish(Arg.Any<int>());
    }

    [Fact]
    public void Subscribe_AfterDispose_ReturnsEmptyDisposable() {
        var target = Substitute.For<IDisposableEvent<int>>();
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
        var target = Substitute.For<IDisposableEvent<int>>();
        var sut = new ForwardingEvent<int>(target);

        sut.Dispose();

        var act = () => sut.ClearHandlers();
        act.Should().NotThrow();
    }

    [Fact]
    public void SnapshotHandlers_WithoutIncludeSelf_ReturnsEmpty() {
        var target = Substitute.For<IDisposableEvent<int>>();
        var sut = new ForwardingEvent<int>(target, ForwardTiming.AfterSelf, ForwardFlags.Subscribe);

        using var snapshot = sut.SnapshotHandlers();

        snapshot.Span.Length.Should().Be(0);
    }

    [Fact]
    public void SnapshotHandlers_WithIncludeSelf_ReturnsHandlers() {
        var target = Substitute.For<IDisposableEvent<int>>();
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
