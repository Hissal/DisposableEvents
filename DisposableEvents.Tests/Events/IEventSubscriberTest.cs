using DisposableEvents.Disposables;

namespace DisposableEvents.Tests.Events;

[TestSubject(typeof(IEventSubscriber<>))]
public class EventSubscriberTest {
    readonly SpySubscriber<int> sut = new();
    
    sealed class SpySubscriber<T> : AbstractSubscriber<T>, IEventSubscriber<T> {
        public IEventHandler<T>? LastSubscribedHandler { get; private set; }

        public override IDisposable Subscribe(IEventHandler<T> handler) {
            LastSubscribedHandler = handler;
            return Disposable.Empty;
        }
    }
    
    [Fact]
    public void SubscribeWithFilter_RoutesToSubscribeWithOnlyHandler_WithFilterApplied() {
        // Arrange
        var handler = Substitute.For<IEventHandler<int>>();
        var filter = Substitute.For<IEventFilter<int>>();

        // Act
        sut.Subscribe(handler, filter);

        // Assert
        sut.LastSubscribedHandler.Should().BeOfType<FilteredEventHandler<int>>()
            .Which.Should().Match<FilteredEventHandler<int>>(fh =>
                fh.GetAnyPrivateFieldOfType<IEventHandler<int>>() == handler &&
                fh.GetAnyPrivateFieldOfType<IEventFilter<int>>() == filter);
    }
    
    [Fact]
    public void SubscribeWithMultipleFiltersAndOrdering_RoutesToSubscribeWithOnlyHandler_WithCombinedFilterApplied() {
        // Arrange
        var handler = Substitute.For<IEventHandler<int>>();
        var filter1 = Substitute.For<IEventFilter<int>>();
        var filter2 = Substitute.For<IEventFilter<int>>();

        // Act
        sut.Subscribe(handler, [filter1, filter2], FilterOrdering.StableSort);

        // Assert
        sut.LastSubscribedHandler.Should().BeOfType<FilteredEventHandler<int>>()
            .Which.Should().Match<FilteredEventHandler<int>>(fh =>
                fh.GetAnyPrivateFieldOfType<IEventHandler<int>>() == handler &&
                fh.GetAnyPrivateFieldOfType<IEventFilter<int>>() is CompositeEventFilter<int>);
    }
}