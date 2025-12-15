using Disposable = DisposableEvents.Disposables.Disposable;

namespace DisposableEvents.Tests.Events;

[TestSubject(typeof(EventCore<>))]
public class EventCoreTest {
    readonly EventCore<int> sut = new();

    [Fact]
    public void Publish_SendsMessageToHandlers() {
        // Arrange
        var handlers = Enumerable.Range(0, 5)
            .Select(_ => Substitute.For<IEventHandler<int>>())
            .ToArray();
        
        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }
        
        // Act
        sut.Publish(69);

        // Assert
        Assert.All(handlers, h => h.Received(1).Handle(69));
    }
    
    [Fact]
    public void DisposedSubscription_Handler_DoesNotReceiveMessages() {
        // Arrange
        var handler = Substitute.For<IEventHandler<int>>();
        var subscription = sut.Subscribe(handler);
        
        // Act
        subscription.Dispose();
        sut.Publish(42);
        
        // Assert
        handler.DidNotReceive().Handle(Arg.Any<int>());
    }

    [Fact]
    public void Publish_AfterClearSubscriptions_DoesNotSendMessageToHandlers() {
        // Arrange
        var handlers = Enumerable.Range(0, 5)
            .Select(_ => Substitute.For<IEventHandler<int>>())
            .ToArray();

        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }
        
        // Act
        sut.ClearHandlers();
        sut.Publish(69);
        
        // Assert
        Assert.All(handlers, h => h.DidNotReceive().Handle(Arg.Any<int>()));
    }
    
    [Fact]
    public void Publish_AfterDispose_DoesNotSendMessageToHandlers() {
        // Arrange
        var handlers = Enumerable.Range(0, 5)
            .Select(_ => Substitute.For<IEventHandler<int>>())
            .ToArray();

        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }
        
        // Act
        sut.Dispose();
        sut.Publish(69);
        
        // Assert
        Assert.All(handlers, h => h.DidNotReceive().Handle(Arg.Any<int>()));
    }

    [Fact]
    public void Subscribe_AfterDispose_ReturnsEmptyDisposable() {
        // Arrange
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Dispose();
        
        // Act
        var subscription = sut.Subscribe(handler);
        
        // Assert
        Assert.Same(Disposable.Empty, subscription);
    }
    
    [Fact]
    public void Subscribe_AfterDispose_HandlerDoesNotReceiveMessages() {
        // Arrange
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Dispose();
        
        // Act
        sut.Subscribe(handler);
        sut.Publish(42);
        
        // Assert
        handler.DidNotReceive().Handle(Arg.Any<int>());
    }
    
    [Fact]
    public void HandlerCount_ReturnsCorrectCount() {
        // Arrange
        var handlers = Enumerable.Range(0, 4)
            .Select(_ => Substitute.For<IEventHandler<int>>())
            .ToArray();

        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }
        
        sut.HandlerCount.Should().Be(handlers.Length);
    }
    
    [Fact]
    public void IsDisposed_ReflectsDisposeState() {
        Assert.False(sut.IsDisposed);
        sut.Dispose();
        Assert.True(sut.IsDisposed);
    }
    
    [Fact]
    public void GetHandlers_ReturnsCurrentHandlers() {
        // Arrange
        var handlers = Enumerable.Range(0, 3)
            .Select(_ => Substitute.For<IEventHandler<int>>())
            .ToArray();

        foreach (var handler in handlers) {
            sut.Subscribe(handler);
        }
        
        // Act
        using var handlerSnapshot = sut.SnapshotHandlers();
        var currentHandlers = handlerSnapshot.Span;
        
        // Assert
        Assert.Equal(handlers, currentHandlers.ToArray());
    }
    
    [Fact]
    public void GetHandlers_AfterDispose_ReturnsEmptyArray() {
        // Arrange
        var handler = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler);
        sut.Dispose();
        
        // Act
        using var handlerSnapshot = sut.SnapshotHandlers();
        var currentHandlers = handlerSnapshot.Span;
        
        // Assert
        Assert.Empty(currentHandlers.ToArray());
    }
    
    [Fact]
    public void GetHandlers_ReturnsCachedHandlers() {
        // Arrange
        var handler1 = Substitute.For<IEventHandler<int>>();
        var handler2 = Substitute.For<IEventHandler<int>>();
        sut.Subscribe(handler1);
        
        // Act
        using var firstSnapshot = sut.SnapshotHandlers();
        var firstCall = firstSnapshot.Span.ToArray();
        
        using var secondSnapshot = sut.SnapshotHandlers();
        var secondCall = secondSnapshot.Span.ToArray();
        
        sut.Subscribe(handler2);
        using var afterSecondSnapshot = sut.SnapshotHandlers();
        var afterSecondSubscribe = afterSecondSnapshot.Span.ToArray();
        
        // Assert
        firstCall.Should().BeEquivalentTo(secondCall);
        afterSecondSubscribe.Should().NotBeEquivalentTo(secondCall);
        afterSecondSubscribe.Should().Contain([handler1, handler2]);
    }
}