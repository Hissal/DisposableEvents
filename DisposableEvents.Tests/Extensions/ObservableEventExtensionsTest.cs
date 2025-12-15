using DisposableEvents.Disposables;

namespace DisposableEvents.Tests.Extensions;

[TestSubject(typeof(ObservableEventExtensions))]
public class ObservableEventExtensionsTest {
    [Fact]
    public void AsObservable_WithoutFilter_SubscribesAndPublishesMessages() {
        // Arrange
        var subscriber = Substitute.For<IEventSubscriber<int>>();
        var observer = Substitute.For<IObserver<int>>();
        IEventHandler<int>? capturedHandler = null;
        
        subscriber.Subscribe(Arg.Do<IEventHandler<int>>(h => capturedHandler = h))
            .Returns(Disposable.Empty);

        // Act
        var observable = subscriber.AsObservable();
        var subscription = observable.Subscribe(observer);
        
        // Assert - subscription was made
        subscriber.Received(1).Subscribe(Arg.Any<IEventHandler<int>>());
        capturedHandler.Should().NotBeNull();
        
        // Act - publish a message
        capturedHandler!.Handle(42);
        
        // Assert - observer received the message
        observer.Received(1).OnNext(42);
    }

    [Fact]
    public void AsObservable_WithoutFilter_DisposesSubscription() {
        // Arrange
        var subscriber = Substitute.For<IEventSubscriber<int>>();
        var observer = Substitute.For<IObserver<int>>();
        var innerSubscription = Substitute.For<IDisposable>();
        
        subscriber.Subscribe(Arg.Any<IEventHandler<int>>())
            .Returns(innerSubscription);

        // Act
        var observable = subscriber.AsObservable();
        var subscription = observable.Subscribe(observer);
        subscription.Dispose();
        
        // Assert
        innerSubscription.Received(1).Dispose();
        observer.Received(1).OnCompleted();
    }

    [Fact]
    public void AsObservable_WithoutFilter_HandlesExceptionInObserver() {
        // Arrange
        var subscriber = Substitute.For<IEventSubscriber<int>>();
        var observer = Substitute.For<IObserver<int>>();
        var exception = new InvalidOperationException("Test exception");
        IEventHandler<int>? capturedHandler = null;
        
        subscriber.Subscribe(Arg.Do<IEventHandler<int>>(h => capturedHandler = h))
            .Returns(Disposable.Empty);
        observer.When(o => o.OnNext(Arg.Any<int>())).Do(_ => throw exception);

        // Act
        var observable = subscriber.AsObservable();
        observable.Subscribe(observer);
        capturedHandler!.Handle(42);
        
        // Assert
        observer.Received(1).OnError(exception);
    }

    [Fact]
    public void AsObservable_WithSingleFilter_SubscribesWithFilter() {
        // Arrange
        var subscriber = Substitute.For<IEventSubscriber<int>>();
        var observer = Substitute.For<IObserver<int>>();
        var filter = Substitute.For<IEventFilter<int>>();
        IEventHandler<int>? capturedHandler = null;
        IEventFilter<int>? capturedFilter = null;
        
        subscriber.Subscribe(Arg.Do<IEventHandler<int>>(h => capturedHandler = h), 
                           Arg.Do<IEventFilter<int>>(f => capturedFilter = f))
            .Returns(Disposable.Empty);

        // Act
        var observable = subscriber.AsObservable(filter);
        observable.Subscribe(observer);
        
        // Assert - subscription was made with filter
        subscriber.Received(1).Subscribe(Arg.Any<IEventHandler<int>>(), filter);
        capturedHandler.Should().NotBeNull();
        capturedFilter.Should().Be(filter);
    }

    [Fact]
    public void AsObservable_WithSingleFilter_DisposesSubscription() {
        // Arrange
        var subscriber = Substitute.For<IEventSubscriber<int>>();
        var observer = Substitute.For<IObserver<int>>();
        var filter = Substitute.For<IEventFilter<int>>();
        var innerSubscription = Substitute.For<IDisposable>();
        
        subscriber.Subscribe(Arg.Any<IEventHandler<int>>(), Arg.Any<IEventFilter<int>>())
            .Returns(innerSubscription);

        // Act
        var observable = subscriber.AsObservable(filter);
        var subscription = observable.Subscribe(observer);
        subscription.Dispose();
        
        // Assert
        innerSubscription.Received(1).Dispose();
        observer.Received(1).OnCompleted();
    }

    [Fact]
    public void AsObservable_WithMultipleFilters_NoFilters_SubscribesWithoutFilter() {
        // Arrange
        var subscriber = Substitute.For<IEventSubscriber<int>>();
        var observer = Substitute.For<IObserver<int>>();
        var filters = Array.Empty<IEventFilter<int>>();

        // Act
        var observable = subscriber.AsObservable(filters);
        observable.Subscribe(observer);
        
        // Assert - when no filters provided, should call the simple Subscribe
        subscriber.Received(1).Subscribe(Arg.Any<IEventHandler<int>>());
        subscriber.DidNotReceive().Subscribe(Arg.Any<IEventHandler<int>>(), Arg.Any<IEventFilter<int>>());
        subscriber.DidNotReceive().Subscribe(Arg.Any<IEventHandler<int>>(), Arg.Any<IEventFilter<int>[]>(), Arg.Any<FilterOrdering>());
    }

    [Fact]
    public void AsObservable_WithMultipleFilters_SingleFilter_SubscribesWithSingleFilter() {
        // Arrange
        var subscriber = Substitute.For<IEventSubscriber<int>>();
        var observer = Substitute.For<IObserver<int>>();
        var filter = Substitute.For<IEventFilter<int>>();
        var filters = new[] { filter };

        // Act
        var observable = subscriber.AsObservable(filters);
        observable.Subscribe(observer);
        
        // Assert - when single filter in array, should call single filter Subscribe
        subscriber.Received(1).Subscribe(Arg.Any<IEventHandler<int>>(), filter);
        subscriber.DidNotReceive().Subscribe(Arg.Any<IEventHandler<int>>());
        subscriber.DidNotReceive().Subscribe(Arg.Any<IEventHandler<int>>(), Arg.Any<IEventFilter<int>[]>(), Arg.Any<FilterOrdering>());
    }

    [Fact]
    public void AsObservable_WithMultipleFilters_MultipleFilters_SubscribesWithFiltersArray() {
        // Arrange
        var subscriber = Substitute.For<IEventSubscriber<int>>();
        var observer = Substitute.For<IObserver<int>>();
        var filter1 = Substitute.For<IEventFilter<int>>();
        var filter2 = Substitute.For<IEventFilter<int>>();
        var filters = new[] { filter1, filter2 };
        IEventFilter<int>[]? capturedFilters = null;

        subscriber.Subscribe(Arg.Any<IEventHandler<int>>(), 
                           Arg.Do<IEventFilter<int>[]>(f => capturedFilters = f), 
                           Arg.Any<FilterOrdering>())
            .Returns(Disposable.Empty);

        // Act
        var observable = subscriber.AsObservable(filters);
        observable.Subscribe(observer);
        
        // Assert - when multiple filters, should call array Subscribe with StableSort
        subscriber.Received(1).Subscribe(Arg.Any<IEventHandler<int>>(), 
            Arg.Is<IEventFilter<int>[]>(f => f.Length == 2 && f[0] == filter1 && f[1] == filter2), 
            FilterOrdering.StableSort);
        capturedFilters.Should().NotBeNull();
        capturedFilters.Should().HaveCount(2);
        capturedFilters![0].Should().Be(filter1);
        capturedFilters[1].Should().Be(filter2);
    }

    [Fact]
    public void AsObservable_ReturnsObservableAdapter() {
        // Arrange
        var subscriber = Substitute.For<IEventSubscriber<int>>();

        // Act
        var observable = subscriber.AsObservable();

        // Assert
        observable.Should().NotBeNull();
        observable.Should().BeAssignableTo<IObservable<int>>();
    }

    [Fact]
    public void AsObservable_WithFilter_ReturnsObservableAdapter() {
        // Arrange
        var subscriber = Substitute.For<IEventSubscriber<int>>();
        var filter = Substitute.For<IEventFilter<int>>();

        // Act
        var observable = subscriber.AsObservable(filter);

        // Assert
        observable.Should().NotBeNull();
        observable.Should().BeAssignableTo<IObservable<int>>();
    }

    [Fact]
    public void AsObservable_WithMultipleFilters_ReturnsObservableAdapter() {
        // Arrange
        var subscriber = Substitute.For<IEventSubscriber<int>>();
        var filter1 = Substitute.For<IEventFilter<int>>();
        var filter2 = Substitute.For<IEventFilter<int>>();

        // Act
        var observable = subscriber.AsObservable(filter1, filter2);

        // Assert
        observable.Should().NotBeNull();
        observable.Should().BeAssignableTo<IObservable<int>>();
    }

    [Fact]
    public void AsObservable_MultipleSubscribers_EachGetsOwnHandler() {
        // Arrange
        var subscriber = Substitute.For<IEventSubscriber<int>>();
        var observer1 = Substitute.For<IObserver<int>>();
        var observer2 = Substitute.For<IObserver<int>>();
        var handlerCount = 0;
        
        subscriber.Subscribe(Arg.Any<IEventHandler<int>>())
            .Returns(_ => {
                handlerCount++;
                return Disposable.Empty;
            });

        // Act
        var observable = subscriber.AsObservable();
        var subscription1 = observable.Subscribe(observer1);
        var subscription2 = observable.Subscribe(observer2);
        
        // Assert - each subscription should create its own handler
        subscriber.Received(2).Subscribe(Arg.Any<IEventHandler<int>>());
        handlerCount.Should().Be(2);
    }
}
