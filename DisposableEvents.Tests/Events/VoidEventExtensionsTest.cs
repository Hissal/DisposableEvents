using DisposableEvents;

namespace DisposableEvents.Tests.Events;

[TestSubject(typeof(VoidEventExtensions))]
public class VoidEventExtensionsTest {
    // Common substitutes used across tests
    readonly IEventPublisher<Void> publisher = Substitute.For<IEventPublisher<Void>>();
    readonly IEventSubscriber<Void> subscriber = Substitute.For<IEventSubscriber<Void>>();
    readonly IEventFilter<Void> f1 = Substitute.For<IEventFilter<Void>>();
    readonly IEventFilter<Void> f2 = Substitute.For<IEventFilter<Void>>();
    readonly IDisposable disp = Substitute.For<IDisposable>();

    [Fact]
    public void Publish_CallsPublisherWithVoidValue() {
        // Act
        VoidEventExtensions.Publish(publisher);

        // Assert
        publisher.Received(1).Publish(Void.Value);
    }

    [Fact]
    public void Subscribe_WithActionAndNoFilters_WrapsActionInVoidEventHandlerAndRoutesToSubscriber() {
        // Arrange
        int callCount = 0;
        subscriber
            .Subscribe(Arg.Any<IEventHandler<Void>>())
            .Returns(ci => {
                var h = ci.Arg<IEventHandler<Void>>();
                h.Handle(Void.Value);
                return disp;
            });

        // Act
        var result = VoidEventExtensions.Subscribe(subscriber, () => callCount++);

        // Assert
        result.Should().Be(disp);
        callCount.Should().Be(1);
        subscriber.Received(1).Subscribe(Arg.Is<IEventHandler<Void>>(h => h is VoidEventHandler));
    }

    [Fact]
    public void Subscribe_WithActionAndFilters_WrapsActionInVoidEventHandlerAndRoutesToSubscriberWithFilters() {
        // Arrange
        int callCount = 0;
        subscriber
            .Subscribe(Arg.Any<IEventHandler<Void>>(), Arg.Is<IEventFilter<Void>[]>(a => a.Length == 2 && a[0] == f1 && a[1] == f2), FilterOrdering.StableSort)
            .Returns(ci => {
                var h = ci.Arg<IEventHandler<Void>>();
                h.Handle(Void.Value);
                return disp;
            });

        // Act
        var result = VoidEventExtensions.Subscribe(subscriber, () => callCount++, f1, f2);

        // Assert
        result.Should().Be(disp);
        callCount.Should().Be(1);
        subscriber.Received(1)
            .Subscribe(
                Arg.Is<IEventHandler<Void>>(h => h is VoidEventHandler),
                Arg.Is<IEventFilter<Void>[]>(a => a.Length == 2 && a[0] == f1 && a[1] == f2),
                FilterOrdering.StableSort);
    }

    [Fact]
    public void Subscribe_WithActionAndPredicate_NoAdditional_CreatesVoidPredicateFilterAndRoutesToSingleFilter() {
        // Arrange
        bool predCalled = false;
        bool Predicate() { predCalled = true; return true; }

        subscriber
            .Subscribe(
                Arg.Any<IEventHandler<Void>>(),
                Arg.Is<IEventFilter<Void>>(f => f is VoidPredicateEventFilter))
            .Returns(ci => {
                var filter = ci.Arg<IEventFilter<Void>>();
                var msg = Void.Value;
                _ = filter.Filter(ref msg);
                return disp;
            });

        int observed = 0;
        var result = VoidEventExtensions.Subscribe(subscriber, () => observed++, Predicate);

        // Assert
        result.Should().Be(disp);
        predCalled.Should().BeTrue();
        subscriber.Received(1)
            .Subscribe(
                Arg.Is<IEventHandler<Void>>(h => h is VoidEventHandler),
                Arg.Is<IEventFilter<Void>>(f => f is VoidPredicateEventFilter));

        // also ensure handler wiring works by invoking via callback above
        observed = 0;
        subscriber.ClearReceivedCalls();
        subscriber
            .Subscribe(
                Arg.Do<IEventHandler<Void>>(h => h.Handle(Void.Value)),
                Arg.Any<IEventFilter<Void>>())
            .Returns(disp);
        _ = VoidEventExtensions.Subscribe(subscriber, () => observed++, Predicate);
        observed.Should().Be(1);
    }

    [Fact]
    public void Subscribe_WithActionAndPredicate_WithAdditional_PredicateIsFirstAndRoutesToSubscriberWithFiltersArray() {
        // Arrange
        bool predCalled = false;
        bool Predicate() { predCalled = true; return true; }

        subscriber
            .Subscribe(
                Arg.Any<IEventHandler<Void>>(),
                Arg.Is<IEventFilter<Void>[]>(a => a.Length == 3 && a[0] is VoidPredicateEventFilter && a[1] == f1 && a[2] == f2),
                FilterOrdering.StableSort)
            .Returns(ci => {
                // trigger predicate
                var arr = ci.Arg<IEventFilter<Void>[]>();
                var msg = Void.Value;
                _ = arr[0].Filter(ref msg);
                return disp;
            });

        // Act
        var result = VoidEventExtensions.Subscribe(subscriber, () => { }, Predicate, f1, f2);

        // Assert
        result.Should().Be(disp);
        predCalled.Should().BeTrue();
        subscriber.Received(1)
            .Subscribe(
                Arg.Is<IEventHandler<Void>>(h => h is VoidEventHandler),
                Arg.Is<IEventFilter<Void>[]>(a => a.Length == 3 && a[0] is VoidPredicateEventFilter && a[1] == f1 && a[2] == f2),
                FilterOrdering.StableSort);
    }

    [Fact]
    public void Subscribe_WithStatefulAction_NoFilters_WrapsStateInVoidEventHandlerAndRoutesToSubscriber() {
        // Arrange
        int state = 5;
        int observed = 0;
        subscriber
            .Subscribe(Arg.Any<IEventHandler<Void>>())
            .Returns(ci => {
                var h = ci.Arg<IEventHandler<Void>>();
                h.Handle(Void.Value);
                return disp;
            });

        // Act
        var result = VoidEventExtensions.Subscribe(subscriber, state, (int s) => observed = s);

        // Assert
        result.Should().Be(disp);
        observed.Should().Be(state);
        subscriber.Received(1).Subscribe(Arg.Is<IEventHandler<Void>>(h => h is VoidEventHandler<int>));
    }

    [Fact]
    public void Subscribe_WithStatefulAction_WithFilters_WrapsStateInVoidEventHandlerAndRoutesToSubscriberWithFilters() {
        // Arrange
        int state = 42;
        int observed = 0;
        subscriber
            .Subscribe(Arg.Any<IEventHandler<Void>>(), Arg.Is<IEventFilter<Void>[]>(a => a.Length == 2 && a[0] == f1 && a[1] == f2), FilterOrdering.StableSort)
            .Returns(ci => {
                var h = ci.Arg<IEventHandler<Void>>();
                h.Handle(Void.Value);
                return disp;
            });

        // Act
        var result = VoidEventExtensions.Subscribe(subscriber, state, (int s) => observed = s, f1, f2);

        // Assert
        result.Should().Be(disp);
        observed.Should().Be(state);
        subscriber.Received(1)
            .Subscribe(
                Arg.Is<IEventHandler<Void>>(h => h is VoidEventHandler<int>),
                Arg.Is<IEventFilter<Void>[]>(a => a.Length == 2 && a[0] == f1 && a[1] == f2),
                FilterOrdering.StableSort);
    }

    [Fact]
    public void Subscribe_WithStatefulActionAndPredicate_NoAdditional_CreatesVoidPredicateAndRoutesToSingleFilter() {
        // Arrange
        int state = 3;
        bool predCalled = false;
        bool Predicate() { predCalled = true; return true; }

        subscriber
            .Subscribe(
                Arg.Any<IEventHandler<Void>>(),
                Arg.Is<IEventFilter<Void>>(f => f is VoidPredicateEventFilter))
            .Returns(ci => {
                var filter = ci.Arg<IEventFilter<Void>>();
                var msg = Void.Value;
                _ = filter.Filter(ref msg);
                return disp;
            });

        // Act
        var result = VoidEventExtensions.Subscribe(subscriber, state, (_) => { }, Predicate);

        // Assert
        result.Should().Be(disp);
        predCalled.Should().BeTrue();
        subscriber.Received(1)
            .Subscribe(
                Arg.Is<IEventHandler<Void>>(h => h is VoidEventHandler<int>),
                Arg.Is<IEventFilter<Void>>(f => f is VoidPredicateEventFilter));
    }

    [Fact]
    public void Subscribe_WithStatefulActionAndPredicate_WithAdditional_PredicateIsFirstAndRoutesToSubscriberWithFiltersArray() {
        // Arrange
        int state = 7;
        bool predCalled = false;
        bool Predicate() { predCalled = true; return true; }

        subscriber
            .Subscribe(
                Arg.Any<IEventHandler<Void>>(),
                Arg.Is<IEventFilter<Void>[]>(a => a.Length == 2 && a[0] is VoidPredicateEventFilter && a[1] == f2),
                FilterOrdering.StableSort)
            .Returns(ci => {
                var arr = ci.Arg<IEventFilter<Void>[]>();
                var msg = Void.Value;
                _ = arr[0].Filter(ref msg);
                return disp;
            });

        // Act
        var result = VoidEventExtensions.Subscribe(subscriber, state, (_) => { }, Predicate, f2);

        // Assert
        result.Should().Be(disp);
        predCalled.Should().BeTrue();
        subscriber.Received(1)
            .Subscribe(
                Arg.Is<IEventHandler<Void>>(h => h is VoidEventHandler<int>),
                Arg.Is<IEventFilter<Void>[]>(a => a.Length == 2 && a[0] is VoidPredicateEventFilter && a[1] == f2),
                FilterOrdering.StableSort);
    }
}
