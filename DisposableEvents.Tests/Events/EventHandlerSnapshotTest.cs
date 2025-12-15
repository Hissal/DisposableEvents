namespace DisposableEvents.Tests.Events;

[TestSubject(typeof(EventHandlerSnapshot<>))]
public class EventHandlerSnapshotTest {
    [Fact]
    public void Empty_ReturnsEmptySnapshot() {
        // Act
        var snapshot = EventHandlerSnapshot<int>.Empty;
        
        // Assert
        snapshot.Length.Should().Be(0);
        snapshot.Span.Length.Should().Be(0);
    }
    
    [Fact]
    public void Constructor_WithEmptySpan_CreatesEmptySnapshot() {
        // Arrange
        var emptySpan = ReadOnlySpan<IEventHandler<int>>.Empty;
        
        // Act
        using var snapshot = new EventHandlerSnapshot<int>(emptySpan);
        
        // Assert
        snapshot.Length.Should().Be(0);
        snapshot.Span.Length.Should().Be(0);
    }
    
    [Fact]
    public void Constructor_WithHandlers_CopiesHandlersToSnapshot() {
        // Arrange
        var handlers = new[] {
            Substitute.For<IEventHandler<int>>(),
            Substitute.For<IEventHandler<int>>(),
            Substitute.For<IEventHandler<int>>()
        };
        
        // Act
        using var snapshot = new EventHandlerSnapshot<int>(handlers);
        
        // Assert
        snapshot.Length.Should().Be(3);
        for (int i = 0; i < handlers.Length; i++) {
            snapshot[i].Should().BeSameAs(handlers[i]);
        }
    }
    
    [Fact]
    public void Length_ReturnsCorrectHandlerCount() {
        // Arrange
        var handlers = new[] {
            Substitute.For<IEventHandler<string>>(),
            Substitute.For<IEventHandler<string>>()
        };
        
        // Act
        using var snapshot = new EventHandlerSnapshot<string>(handlers);
        
        // Assert
        snapshot.Length.Should().Be(2);
    }
    
    [Fact]
    public void Indexer_ReturnsCorrectHandler() {
        // Arrange
        var handler1 = Substitute.For<IEventHandler<int>>();
        var handler2 = Substitute.For<IEventHandler<int>>();
        var handler3 = Substitute.For<IEventHandler<int>>();
        var handlers = new[] { handler1, handler2, handler3 };
        
        // Act
        using var snapshot = new EventHandlerSnapshot<int>(handlers);
        
        // Assert
        snapshot[0].Should().BeSameAs(handler1);
        snapshot[1].Should().BeSameAs(handler2);
        snapshot[2].Should().BeSameAs(handler3);
    }
    
    [Fact]
    public void Span_ReturnsValidSpan() {
        // Arrange
        var handlers = new[] {
            Substitute.For<IEventHandler<int>>(),
            Substitute.For<IEventHandler<int>>()
        };
        
        // Act
        using var snapshot = new EventHandlerSnapshot<int>(handlers);
        var span = snapshot.Span;
        
        // Assert
        span.Length.Should().Be(2);
        span[0].Should().BeSameAs(handlers[0]);
        span[1].Should().BeSameAs(handlers[1]);
    }
    
    [Fact]
    public void GetEnumerator_AllowsEnumerationOfHandlers() {
        // Arrange
        var handlers = new[] {
            Substitute.For<IEventHandler<int>>(),
            Substitute.For<IEventHandler<int>>(),
            Substitute.For<IEventHandler<int>>()
        };
        
        // Act
        using var snapshot = new EventHandlerSnapshot<int>(handlers);
        var enumeratedHandlers = new List<IEventHandler<int>>();
        
        foreach (var handler in snapshot) {
            enumeratedHandlers.Add(handler);
        }
        
        // Assert
        enumeratedHandlers.Should().HaveCount(3);
        enumeratedHandlers.Should().ContainInOrder(handlers);
    }
    
    [Fact]
    public void Dispose_ReleasesPooledResources() {
        // Arrange
        var handlers = new[] {
            Substitute.For<IEventHandler<int>>(),
            Substitute.For<IEventHandler<int>>()
        };
        var snapshot = new EventHandlerSnapshot<int>(handlers);
        
        // Act
        snapshot.Dispose();
        
        // Assert - After disposal, span should be empty
        snapshot.Span.Length.Should().Be(0);
    }
    
    [Fact]
    public void Dispose_MultipleTimes_IsIdempotent() {
        // Arrange
        var handlers = new[] {
            Substitute.For<IEventHandler<int>>(),
            Substitute.For<IEventHandler<int>>()
        };
        var snapshot = new EventHandlerSnapshot<int>(handlers);
        
        // Act
        snapshot.Dispose();
        snapshot.Dispose();
        snapshot.Dispose();
        
        // Assert - No exception should be thrown, span should be empty
        snapshot.Span.Length.Should().Be(0);
    }
    
    [Fact]
    public void Span_AfterDisposal_ReturnsEmptySpan() {
        // Arrange
        var handlers = new[] {
            Substitute.For<IEventHandler<int>>(),
            Substitute.For<IEventHandler<int>>()
        };
        var snapshot = new EventHandlerSnapshot<int>(handlers);
        
        // Act
        snapshot.Dispose();
        var spanAfterDisposal = snapshot.Span;
        
        // Assert
        spanAfterDisposal.Length.Should().Be(0);
    }
    
    [Fact]
    public void Empty_CanBeEnumerated() {
        // Act
        var snapshot = EventHandlerSnapshot<int>.Empty;
        var count = 0;
        
        foreach (var _ in snapshot) {
            count++;
        }
        
        // Assert
        count.Should().Be(0);
    }
    
    [Fact]
    public void Constructor_WithSingleHandler_WorksCorrectly() {
        // Arrange
        var handler = Substitute.For<IEventHandler<string>>();
        var handlers = new[] { handler };
        
        // Act
        using var snapshot = new EventHandlerSnapshot<string>(handlers);
        
        // Assert
        snapshot.Length.Should().Be(1);
        snapshot[0].Should().BeSameAs(handler);
    }
}
