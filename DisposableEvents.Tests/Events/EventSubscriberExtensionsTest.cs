using DisposableEvents;
using System.Linq;

namespace DisposableEvents.Tests.Events;

[TestSubject(typeof(EventSubscriberExtensions))]
public class EventSubscriberExtensionsTest {
    // Common substitutes used across tests
    readonly IEventSubscriber<int> subscriber = Substitute.For<IEventSubscriber<int>>();
    readonly IEventHandler<int> handler = Substitute.For<IEventHandler<int>>();
    readonly IEventFilter<int> f1 = Substitute.For<IEventFilter<int>>();
    readonly IEventFilter<int> f2 = Substitute.For<IEventFilter<int>>();
    readonly IDisposable disp = Substitute.For<IDisposable>();

    [Fact]
    public void Subscribe_WithHandlerAndSingleFilter_RoutesToSubscriberOverload() {
        // Arrange
        subscriber.Subscribe(handler, f1).Returns(disp);

        // Act
        var result = EventSubscriberExtensions.Subscribe(subscriber, handler, f1);

        // Assert
        result.Should().Be(disp);
        subscriber.Received(1).Subscribe(handler, f1);
        subscriber.DidNotReceive().Subscribe(Arg.Any<IEventHandler<int>>(), Arg.Any<IEventFilter<int>[]>(), Arg.Any<FilterOrdering>());
    }

    [Fact]
    public void Subscribe_WithHandlerFiltersAndOrdering_RoutesToSubscriberOverload() {
        // Arrange
        var filters = new[] { f1, f2 };
        subscriber.Subscribe(handler, filters, FilterOrdering.UnstableSort).Returns(disp);

        // Act
        var result = EventSubscriberExtensions.Subscribe(subscriber, handler, filters, FilterOrdering.UnstableSort);

        // Assert
        result.Should().Be(disp);
        subscriber.Received(1).Subscribe(handler, filters, FilterOrdering.UnstableSort);
    }

    [Fact]
    public void Subscribe_WithHandlerAndNoFilters_RoutesToNoFilterOverload() {
        // Arrange
        subscriber.Subscribe(handler).Returns(disp);

        // Act
        var result = EventSubscriberExtensions.Subscribe(subscriber, handler);

        // Assert
        result.Should().Be(disp);
        subscriber.Received(1).Subscribe(handler);
        subscriber.DidNotReceive().Subscribe(Arg.Any<IEventHandler<int>>(), Arg.Any<IEventFilter<int>>());
    }

    [Fact]
    public void Subscribe_WithHandlerAndMultipleParamsFilters_RoutesToFiltersWithStableOrdering() {
        // Arrange
        subscriber
            .Subscribe(handler, Arg.Is<IEventFilter<int>[]>(a => a.Length == 2 && a[0] == f1 && a[1] == f2), FilterOrdering.StableSort)
            .Returns(disp);

        // Act
        var result = EventSubscriberExtensions.Subscribe(subscriber, handler, f1, f2);

        // Assert
        result.Should().Be(disp);
        subscriber.Received(1)
            .Subscribe(handler, Arg.Is<IEventFilter<int>[]>(a => a.Length == 2 && a[0] == f1 && a[1] == f2), FilterOrdering.StableSort);
    }

    [Fact]
    public void Subscribe_WithActionAndMultipleFilters_WrapsActionAndRoutesToFiltersWithStableOrdering() {
        // Arrange
        int received = 0;
        subscriber
            .Subscribe(
                Arg.Any<IEventHandler<int>>(),
                Arg.Is<IEventFilter<int>[]>(a => a.SequenceEqual(new[] { f1, f2 })),
                FilterOrdering.StableSort)
            .Returns(ci => {
                var h = ci.Arg<IEventHandler<int>>();
                h.Handle(42);
                return disp;
            });

        // Act
        var result = EventSubscriberExtensions.Subscribe(subscriber, x => received = x, f1, f2);

        // Assert
        result.Should().Be(disp);
        received.Should().Be(42);
        subscriber.Received(1)
            .Subscribe(Arg.Any<IEventHandler<int>>(), Arg.Is<IEventFilter<int>[]>(a => a.SequenceEqual(new[] { f1, f2 })), FilterOrdering.StableSort);
    }

    [Fact]
    public void Subscribe_WithActionAndPredicate_NoAdditional_CreatesPredicateFilterAndRoutesToSingleFilter() {
        // Arrange
        bool predCalled = false;
        bool Predicate(int m) { predCalled = true; return true; }

        subscriber
            .Subscribe(
                Arg.Any<IEventHandler<int>>(),
                Arg.Is<IEventFilter<int>>(f => f is PredicateEventFilter<int>))
            .Returns(ci => {
                var filter = ci.Arg<IEventFilter<int>>();
                int msg = 7;
                _ = filter.Filter(ref msg);
                return disp;
            });

        int observed = 0;
        var result = EventSubscriberExtensions.Subscribe(subscriber, x => observed = x, Predicate);

        // Assert
        result.Should().Be(disp);
        predCalled.Should().BeTrue();
        subscriber.Received(1)
            .Subscribe(Arg.Any<IEventHandler<int>>(), Arg.Is<IEventFilter<int>>(f => f is PredicateEventFilter<int>));
        subscriber.DidNotReceive().Subscribe(Arg.Any<IEventHandler<int>>(), Arg.Any<IEventFilter<int>[]>(), Arg.Any<FilterOrdering>());

        // also ensure handler wiring works by invoking via callback above
        observed = 0;
        subscriber.ClearReceivedCalls();
        subscriber
            .Subscribe(Arg.Do<IEventHandler<int>>(h => h.Handle(99)), Arg.Any<IEventFilter<int>>())
            .Returns(disp);
        _ = EventSubscriberExtensions.Subscribe(subscriber, x => observed = x, Predicate);
        observed.Should().Be(99);
    }

    [Fact]
    public void Subscribe_WithActionAndPredicate_WithAdditional_PredicateIsFirstAndRoutesToFiltersWithStableOrdering() {
        // Arrange
        bool predCalled = false;
        bool Predicate(int m) { predCalled = true; return true; }

        subscriber
            .Subscribe(
                Arg.Any<IEventHandler<int>>(),
                Arg.Is<IEventFilter<int>[]>(a => a.Length == 3 && a[0] is PredicateEventFilter<int> && a[1] == f1 && a[2] == f2),
                FilterOrdering.StableSort)
            .Returns(ci => {
                // trigger predicate
                var arr = ci.Arg<IEventFilter<int>[]>() ;
                int msg = 1;
                _ = arr[0].Filter(ref msg);
                return disp;
            });

        // Act
        var result = EventSubscriberExtensions.Subscribe(subscriber, _ => { }, Predicate, f1, f2);

        // Assert
        result.Should().Be(disp);
        predCalled.Should().BeTrue();
        subscriber.Received(1)
            .Subscribe(
                Arg.Any<IEventHandler<int>>(),
                Arg.Is<IEventFilter<int>[]>(a => a.Length == 3 && a[0] is PredicateEventFilter<int> && a[1] == f1 && a[2] == f2),
                FilterOrdering.StableSort);
    }

    [Fact]
    public void Subscribe_WithStatefulAction_NoFilters_WrapsStateAndRoutesToNoFilterOverload() {
        // Arrange
        int state = 5;
        int observed = 0;
        subscriber
            .Subscribe(Arg.Any<IEventHandler<int>>())
            .Returns(ci => {
                var h = ci.Arg<IEventHandler<int>>();
                h.Handle(7);
                return disp;
            });

        // Act
        var result = EventSubscriberExtensions.Subscribe(subscriber, state, (int s, int m) => observed = s + m);

        // Assert
        result.Should().Be(disp);
        observed.Should().Be(state + 7);
        subscriber.Received(1).Subscribe(Arg.Any<IEventHandler<int>>());
    }

    [Fact]
    public void Subscribe_WithStatefulActionAndMessagePredicate_NoAdditional_CreatesPredicateAndRoutesToSingleFilter() {
        // Arrange
        int state = 3;
        bool predCalled = false;
        bool Predicate(int s, int m) { predCalled = true; return s + m > 0; }

        subscriber
            .Subscribe(
                Arg.Any<IEventHandler<int>>(),
                Arg.Is<IEventFilter<int>>(f => f is PredicateEventFilter<int, int>))
            .Returns(ci => {
                var filter = ci.Arg<IEventFilter<int>>();
                int msg = 10;
                _ = filter.Filter(ref msg);
                return disp;
            });

        // Act
        var result = EventSubscriberExtensions.Subscribe(subscriber, state, (_, _) => { }, Predicate);

        // Assert
        result.Should().Be(disp);
        predCalled.Should().BeTrue();
        subscriber.Received(1)
            .Subscribe(Arg.Any<IEventHandler<int>>(), Arg.Is<IEventFilter<int>>(f => f is PredicateEventFilter<int, int>));
        subscriber.DidNotReceive().Subscribe(Arg.Any<IEventHandler<int>>(), Arg.Any<IEventFilter<int>[]>(), Arg.Any<FilterOrdering>());
    }

    [Fact]
    public void Subscribe_WithStatefulActionAndMessagePredicate_WithAdditional_PredicateIsFirstAndRoutesToFiltersWithStableOrdering() {
        // Arrange
        int state = 3;

        subscriber
            .Subscribe(
                Arg.Any<IEventHandler<int>>(),
                Arg.Is<IEventFilter<int>[]>(a => a.Length == 2 && a[0] is PredicateEventFilter<int> && a[1] == f1),
                FilterOrdering.StableSort)
            .Returns(disp);

        // Act
        var result = EventSubscriberExtensions.Subscribe(subscriber, state, (_, _) => { }, _ => true, f1);

        // Assert
        result.Should().Be(disp);
        subscriber.Received(1)
            .Subscribe(
                Arg.Any<IEventHandler<int>>(),
                Arg.Is<IEventFilter<int>[]>(a => a.Length == 2 && a[0] is PredicateEventFilter<int> && a[1] == f1),
                FilterOrdering.StableSort);
    }

    [Fact]
    public void Subscribe_WithStatefulActionAndStateMessagePredicate_WithAdditional_PredicateIsFirstAndRoutesToFiltersWithStableOrdering() {
        // Arrange
        int state = 7;
        int seenState = 0;
        int seenMsg = 0;
        bool Predicate(int s, int m) { seenState = s; seenMsg = m; return true; }

        subscriber
            .Subscribe(
                Arg.Any<IEventHandler<int>>(),
                Arg.Is<IEventFilter<int>[]>(a => a.Length == 2 && a[0] is PredicateEventFilter<int, int> && a[1] == f2),
                FilterOrdering.StableSort)
            .Returns(ci => {
                var arr = ci.Arg<IEventFilter<int>[]>() ;
                int msg = 11;
                _ = arr[0].Filter(ref msg);
                return disp;
            });

        // Act
        var result = EventSubscriberExtensions.Subscribe(subscriber, state, (_, _) => { }, Predicate, f2);

        // Assert
        result.Should().Be(disp);
        seenState.Should().Be(state);
        seenMsg.Should().Be(11);
        subscriber.Received(1)
            .Subscribe(
                Arg.Any<IEventHandler<int>>(),
                Arg.Is<IEventFilter<int>[]>(a => a.Length == 2 && a[0] is PredicateEventFilter<int, int> && a[1] == f2),
                FilterOrdering.StableSort);
    }
}