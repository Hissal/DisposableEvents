using Disposable = DisposableEvents.Disposables.Disposable;

namespace DisposableEvents.Tests.Internal;

[TestSubject(typeof(LazyInnerEvent<>))]
public class LazyInnerEventTest {
    [Fact]
    public void Constructor_WithoutParameter_UsesGlobalConfig() {
        var sut = new LazyInnerEvent<int>();
        sut.Should().NotBeNull();
        sut.IsDisposed.Should().BeFalse();
        sut.HandlerCount.Should().Be(0);
    }
    
    [Fact]
    public void Constructor_WithExpectedSubscriberCount_InitializesCorrectly() {
        var sut = new LazyInnerEvent<int>(10);
        sut.Should().NotBeNull();
        sut.IsDisposed.Should().BeFalse();
        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void Publish_BeforeMaterialization_DoesNothing() {
        var sut = new LazyInnerEvent<int>();
        sut.Publish(42);
        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void Publish_AfterSubscription_SendsMessageToHandlers() {
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);
        sut.Publish(69);
        handler.Received(1).Handle(69);
    }

    [Fact]
    public void Subscribe_MaterializesCore_AndReturnsDisposable() {
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        var subscription = sut.Subscribe(handler);
        subscription.Should().NotBeNull();
        subscription.Should().NotBe(Disposable.Empty);
        sut.HandlerCount.Should().Be(1);
    }

    [Fact]
    public void Subscribe_WithFilter_MaterializesCore_AndReturnsDisposable() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        var filter = Substitute.For<IEventFilter<int>>();
        filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        
        // Act
        var subscription = sut.Subscribe(handler, filter);
        
        // Assert
        subscription.Should().NotBeNull();
        subscription.Should().NotBe(Disposable.Empty);
        sut.HandlerCount.Should().Be(1);
    }

    [Fact]
    public void Subscribe_WithMultipleFilters_MaterializesCore_AndReturnsDisposable() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        var filter1 = Substitute.For<IEventFilter<int>>();
        var filter2 = Substitute.For<IEventFilter<int>>();
        filter1.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        filter2.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        
        // Act
        var subscription = sut.Subscribe(handler, new[] { filter1, filter2 }, FilterOrdering.KeepOriginal);
        
        // Assert
        subscription.Should().NotBeNull();
        subscription.Should().NotBe(Disposable.Empty);
        sut.HandlerCount.Should().Be(1);
    }

    [Fact]
    public void Subscribe_AfterDispose_ReturnsEmptyDisposable() {
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Dispose();
        var subscription = sut.Subscribe(handler);
        subscription.Should().Be(Disposable.Empty);
        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void Subscribe_AfterDispose_HandlerDoesNotReceiveMessages() {
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Dispose();
        sut.Subscribe(handler);
        sut.Publish(42);
        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void Publish_AfterDispose_DoesNotSendMessageToHandlers() {
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);
        sut.Dispose();
        sut.Publish(69);
        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void Publish_AfterDisposeBeforeMaterialization_DoesNothing() {
        var sut = new LazyInnerEvent<int>();
        sut.Dispose();
        sut.Publish(42);
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void HandlerCount_BeforeMaterialization_ReturnsZero() {
        var sut = new LazyInnerEvent<int>();
        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void HandlerCount_AfterSubscription_ReturnsCorrectCount() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handlers = Enumerable.Range(0, 3)
            .Select(_ => Substitute.For<IEventHandler<int>>())
            .ToArray();
        
        // Act
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }
        
        // Assert
        sut.HandlerCount.Should().Be(3);
    }

    [Fact]
    public void HandlerCount_AfterUnsubscribe_ReturnsCorrectCount() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handler1 = Substitute.For<IEventHandler<int>>();
        var handler2 = Substitute.For<IEventHandler<int>>();
        var subscription1 = sut.Subscribe(handler1);
        var subscription2 = sut.Subscribe(handler2);
        
        // Act
        subscription1.Dispose();
        
        // Assert
        sut.HandlerCount.Should().Be(1);
    }

    [Fact]
    public void IsDisposed_InitiallyFalse() {
        var sut = new LazyInnerEvent<int>();
        sut.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void IsDisposed_AfterDispose_ReturnsTrue() {
        var sut = new LazyInnerEvent<int>();
        sut.Dispose();
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void IsDisposed_AfterDisposeBeforeMaterialization_ReturnsTrue() {
        var sut = new LazyInnerEvent<int>();
        sut.Dispose();
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void IsDisposed_AfterDisposeOfMaterializedCore_ReturnsTrue() {
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);
        sut.Dispose();
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void SnapshotHandlers_BeforeMaterialization_ReturnsEmpty() {
        var sut = new LazyInnerEvent<int>();
        using var snapshot = sut.SnapshotHandlers();
        snapshot.Span.ToArray().Should().BeEmpty();
    }

    [Fact]
    public void SnapshotHandlers_AfterSubscription_ReturnsHandlers() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handlers = Enumerable.Range(0, 3)
            .Select(_ => Substitute.For<IEventHandler<int>>())
            .ToArray();
        
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }
        
        // Act
        using var snapshot = sut.SnapshotHandlers();
        var currentHandlers = snapshot.Span.ToArray();
        
        // Assert
        currentHandlers.Should().HaveCount(3);
        currentHandlers.Should().Contain(handlers);
    }

    [Fact]
    public void ClearHandlers_BeforeMaterialization_DoesNothing() {
        var sut = new LazyInnerEvent<int>();
        sut.ClearHandlers();
        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void ClearHandlers_AfterSubscription_RemovesAllHandlers() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handlers = Enumerable.Range(0, 3)
            .Select(_ => Substitute.For<IEventHandler<int>>())
            .ToArray();
        
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }
        
        // Act
        sut.ClearHandlers();
        
        // Assert
        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void ClearHandlers_AfterDispose_DoesNothing() {
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);
        sut.Dispose();
        sut.ClearHandlers();
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_Multiple_DoesNotThrow() {
        var sut = new LazyInnerEvent<int>();
        sut.Dispose();
        sut.Dispose();
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_WithSubscribedHandlers_DisposesCore() {
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);
        sut.Dispose();
        sut.IsDisposed.Should().BeTrue();
        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void Next_BeforeMaterialization_ReturnsNull() {
        var sut = new LazyInnerEvent<int>();
        var next = sut.Next;
        next.Should().BeNull();
    }

    [Fact]
    public void Next_AfterSetNext_ReturnsSetValue() {
        var sut = new LazyInnerEvent<int>();
        var nextEvent = new LazyInnerEvent<int>();
        sut.SetNext(nextEvent);
        var next = sut.Next;
        next.Should().Be(nextEvent);
    }

    [Fact]
    public void SetNext_BeforeMaterialization_SetsCore() {
        var sut = new LazyInnerEvent<int>();
        var nextEvent = new LazyInnerEvent<int>();
        sut.SetNext(nextEvent);
        sut.Next.Should().Be(nextEvent);
    }

    [Fact]
    public void SetNext_AfterMaterialization_ThrowsInvalidOperationException() {
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);
        var nextEvent = new LazyInnerEvent<int>();
        var act = () => sut.SetNext(nextEvent);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Inner already materialized; cannot set after first use.");
    }

    [Fact]
    public void SetNext_AfterDispose_ThrowsObjectDisposedException() {
        var sut = new LazyInnerEvent<int>();
        sut.Dispose();
        var nextEvent = new LazyInnerEvent<int>();
        var act = () => sut.SetNext(nextEvent);
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void SetNext_AfterPublishWithoutHandlers_SucceedsBecauseNoMaterialization() {
        var sut = new LazyInnerEvent<int>();
        sut.Publish(42);
        var nextEvent = new LazyInnerEvent<int>();
        sut.SetNext(nextEvent);
        sut.Next.Should().Be(nextEvent);
    }

    [Fact]
    public void Publish_ThroughSetNextPipeline_SendsMessageToHandlers() {
        var sut = new LazyInnerEvent<int>();
        var nextEvent = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.SetNext(nextEvent);
        nextEvent.Subscribe(handler);
        sut.Publish(69);
        handler.Received(1).Handle(69);
    }

    [Fact]
    public void Subscribe_ThroughPipeline_AddsHandlerToSetNext() {
        var sut = new LazyInnerEvent<int>();
        var nextEvent = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.SetNext(nextEvent);
        sut.Subscribe(handler);
        sut.HandlerCount.Should().Be(1);
    }
}
