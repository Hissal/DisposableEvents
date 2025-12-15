using Disposable = DisposableEvents.Disposables.Disposable;

namespace DisposableEvents.Tests.Internal;

[TestSubject(typeof(LazyInnerEvent<>))]
public class LazyInnerEventTest {
    [Fact]
    public void Constructor_WithoutParameter_UsesGlobalConfig() {
        // Arrange & Act
        var sut = new LazyInnerEvent<int>();
        
        // Assert
        sut.Should().NotBeNull();
        sut.IsDisposed.Should().BeFalse();
        sut.HandlerCount.Should().Be(0);
    }
    
    [Fact]
    public void Constructor_WithExpectedSubscriberCount_InitializesCorrectly() {
        // Arrange & Act
        var sut = new LazyInnerEvent<int>(10);
        
        // Assert
        sut.Should().NotBeNull();
        sut.IsDisposed.Should().BeFalse();
        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void Publish_BeforeMaterialization_DoesNothing() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        
        // Act
        sut.Publish(42);
        
        // Assert
        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void Publish_AfterSubscription_SendsMessageToHandlers() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);
        
        // Act
        sut.Publish(69);
        
        // Assert
        handler.Received(1).Handle(69);
    }

    [Fact]
    public void Subscribe_MaterializesCore_AndReturnsDisposable() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        
        // Act
        var subscription = sut.Subscribe(handler);
        
        // Assert
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
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Dispose();
        
        // Act
        var subscription = sut.Subscribe(handler);
        
        // Assert
        subscription.Should().Be(Disposable.Empty);
        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void Subscribe_AfterDispose_HandlerDoesNotReceiveMessages() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Dispose();
        
        // Act
        sut.Subscribe(handler);
        sut.Publish(42);
        
        // Assert
        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void Publish_AfterDispose_DoesNotSendMessageToHandlers() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);
        
        // Act
        sut.Dispose();
        sut.Publish(69);
        
        // Assert
        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void Publish_AfterDisposeBeforeMaterialization_DoesNothing() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        sut.Dispose();
        
        // Act
        sut.Publish(42);
        
        // Assert - Should not throw
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void HandlerCount_BeforeMaterialization_ReturnsZero() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        
        // Act & Assert
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
        // Arrange & Act
        var sut = new LazyInnerEvent<int>();
        
        // Assert
        sut.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void IsDisposed_AfterDispose_ReturnsTrue() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        
        // Act
        sut.Dispose();
        
        // Assert
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void IsDisposed_AfterDisposeBeforeMaterialization_ReturnsTrue() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        
        // Act
        sut.Dispose();
        
        // Assert
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void IsDisposed_AfterDisposeOfMaterializedCore_ReturnsTrue() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);
        
        // Act
        sut.Dispose();
        
        // Assert
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void SnapshotHandlers_BeforeMaterialization_ReturnsEmpty() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        
        // Act
        using var snapshot = sut.SnapshotHandlers();
        
        // Assert
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
        // Arrange
        var sut = new LazyInnerEvent<int>();
        
        // Act
        sut.ClearHandlers();
        
        // Assert
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
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);
        sut.Dispose();
        
        // Act
        sut.ClearHandlers();
        
        // Assert - Should not throw
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_Multiple_DoesNotThrow() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        
        // Act
        sut.Dispose();
        sut.Dispose();
        
        // Assert
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_WithSubscribedHandlers_DisposesCore() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);
        
        // Act
        sut.Dispose();
        
        // Assert
        sut.IsDisposed.Should().BeTrue();
        sut.HandlerCount.Should().Be(0);
    }

    [Fact]
    public void Next_BeforeMaterialization_ReturnsNull() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        
        // Act
        var next = sut.Next;
        
        // Assert
        next.Should().BeNull();
    }

    [Fact]
    public void Next_AfterSetNext_ReturnsSetValue() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var nextEvent = new LazyInnerEvent<int>();
        
        // Act
        sut.SetNext(nextEvent);
        var next = sut.Next;
        
        // Assert
        next.Should().Be(nextEvent);
    }

    [Fact]
    public void SetNext_BeforeMaterialization_SetsCore() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var nextEvent = new LazyInnerEvent<int>();
        
        // Act
        sut.SetNext(nextEvent);
        
        // Assert
        sut.Next.Should().Be(nextEvent);
    }

    [Fact]
    public void SetNext_AfterMaterialization_ThrowsInvalidOperationException() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler); // This materializes the core
        var nextEvent = new LazyInnerEvent<int>();
        
        // Act
        var act = () => sut.SetNext(nextEvent);
        
        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Inner already materialized; cannot set after first use.");
    }

    [Fact]
    public void SetNext_AfterDispose_ThrowsObjectDisposedException() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        sut.Dispose();
        var nextEvent = new LazyInnerEvent<int>();
        
        // Act
        var act = () => sut.SetNext(nextEvent);
        
        // Assert
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void SetNext_AfterPublishWithoutHandlers_SucceedsBecauseNoMaterialization() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        sut.Publish(42); // This doesn't materialize without handlers
        var nextEvent = new LazyInnerEvent<int>();
        
        // Act - SetNext should succeed because publish without handlers doesn't materialize
        sut.SetNext(nextEvent);
        
        // Assert
        sut.Next.Should().Be(nextEvent);
    }

    [Fact]
    public void Publish_ThroughSetNextPipeline_SendsMessageToHandlers() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var nextEvent = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        
        sut.SetNext(nextEvent);
        nextEvent.Subscribe(handler);
        
        // Act
        sut.Publish(69);
        
        // Assert
        handler.Received(1).Handle(69);
    }

    [Fact]
    public void Subscribe_ThroughPipeline_AddsHandlerToSetNext() {
        // Arrange
        var sut = new LazyInnerEvent<int>();
        var nextEvent = new LazyInnerEvent<int>();
        var handler = Substitute.For<IEventHandler<int>>();
        
        sut.SetNext(nextEvent);
        
        // Act
        sut.Subscribe(handler);
        
        // Assert
        sut.HandlerCount.Should().Be(1);
    }
}
