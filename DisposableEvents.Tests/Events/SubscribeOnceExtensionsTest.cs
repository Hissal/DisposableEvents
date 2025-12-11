namespace DisposableEvents.Tests.Events;

[TestSubject(typeof(SubscribeOnceExtensions))]
public class SubscribeOnceExtensionsTest {
    readonly DisposableEvent<int> evt = new();
    readonly IEventHandler<int> handler = Substitute.For<IEventHandler<int>>();
    readonly IEventFilter<int>[] filters = Enumerable.Range(0, 2)
        .Select(_ => Substitute.For<IEventFilter<int>>())
        .ToArray();
    
    readonly DisposableEvent<Void> voidEvt = new();
    readonly IEventFilter<Void>[] voidFilters = Enumerable.Range(0, 2)
        .Select(_ => Substitute.For<IEventFilter<Void>>())
        .ToArray();
    
    public SubscribeOnceExtensionsTest() {
        foreach (var filter in filters) {
            filter.Filter(ref Arg.Any<int>()).Returns(callInfo => {
                callInfo[0] = 42;
                return FilterResult.Pass;
            });
        }
        
        foreach (var filter in voidFilters) {
            filter.Filter(ref Arg.Any<Void>()).Returns(FilterResult.Pass);
        }
    }
    
    [Fact]
    public void SubscribeOnce_WithHandler_Works() {
        evt.SubscribeOnce(handler);
        evt.Publish(42);
        evt.Publish(42);
        
        handler.Received(1).Handle(42);
    }

    [Fact]
    public void SubscribeOnce_WithHandlerAndFilter_Works() {
        evt.SubscribeOnce(handler, filters[0]);
        evt.Publish(42);
        evt.Publish(42);
        
        handler.Received(1).Handle(42);
        filters[0].Received(1).Filter(ref Arg.Any<int>());
    }

    [Fact]
    public void SubscribeOnce_WithHandlerAndFilters_Works() {
        evt.SubscribeOnce(handler, filters);
        evt.Publish(42);
        evt.Publish(42);
        
        handler.Received(1).Handle(42);
        foreach (var filter in filters) {
            filter.Received(1).Filter(ref Arg.Any<int>());
        }
    }
    
    [Fact]
    public void SubscribeOnce_WithHandlerAndFiltersAndFilterOrdering_Works() {
        evt.SubscribeOnce(handler, filters, FilterOrdering.StableSort);
        evt.Publish(42);
        evt.Publish(42);
        
        handler.Received(1).Handle(42);
        foreach (var filter in filters) {
            filter.Received(1).Filter(ref Arg.Any<int>());
        }
    }

    [Fact]
    public void SubscribeOnce_WithActionHandler_Works() {
        var action = Substitute.For<Action<int>>();
        
        evt.SubscribeOnce(action);
        evt.Publish(42);
        evt.Publish(42);
        
        action.Received(1).Invoke(42);
    }
    
    [Fact]
    public void SubscribeOnce_WithActionHandlerAndFilters_Works() {
        var action = Substitute.For<Action<int>>();
        
        evt.SubscribeOnce(action, filters);
        evt.Publish(42);
        evt.Publish(42);
        
        action.Received(1).Invoke(42);
        foreach (var filter in filters) {
            filter.Received(1).Filter(ref Arg.Any<int>());
        }
    }
    
    [Fact]
    public void SubscribeOnce_WithActionHandlerAndPredicateFilter_Works() {
        var action = Substitute.For<Action<int>>();
        var filter = Substitute.For<Func<int, bool>>();
        filter.Invoke(42).Returns(true);
        
        evt.SubscribeOnce(action, filter);
        evt.Publish(42);
        evt.Publish(42);
        
        action.Received(1).Invoke(42);
        filter.Received(1).Invoke(42);
    }

    [Fact]
    public void SubscribeOnce_WithActionHandlerAndPredicateFilterAndAdditionalFilters_Works() {
        var action = Substitute.For<Action<int>>();
        var predicateFilter = Substitute.For<Func<int, bool>>();
        predicateFilter.Invoke(42).Returns(true);
        
        evt.SubscribeOnce(action, predicateFilter, filters);
        evt.Publish(42);
        evt.Publish(42);

        action.Received(1).Invoke(42);
        predicateFilter.Received(1).Invoke(42);
        foreach (var filter in filters) {
            filter.Received(1).Filter(ref Arg.Any<int>());
        }
    }
    
    // ===== Stateful Overloads ===== //
    [Fact]
    public void SubscribeOnce_Stateful_WithActionHandler_Works() {
        var action = Substitute.For<Action<int, int>>();
        
        evt.SubscribeOnce(1, action);
        evt.Publish(42);
        evt.Publish(42);
        
        action.Received(1).Invoke(1, 42);
    }
    
    [Fact]
    public void SubscribeOnce_Stateful_WithActionHandlerAndFilters_Works() {
        var action = Substitute.For<Action<int, int>>();
        
        evt.SubscribeOnce(1, action, filters);
        evt.Publish(42);
        evt.Publish(42);
        
        action.Received(1).Invoke(1, 42);
        foreach (var filter in filters) {
            filter.Received(1).Filter(ref Arg.Any<int>());
        }
    }
    
    [Fact]
    public void SubscribeOnce_Stateful_WithActionHandlerAndPredicateFilter_Works() {
        var action = Substitute.For<Action<int, int>>();
        var filter = Substitute.For<Func<int, int, bool>>();
        filter.Invoke(1, 42).Returns(true);
        
        evt.SubscribeOnce(1, action, filter);
        evt.Publish(42);
        evt.Publish(42);
        
        action.Received(1).Invoke(1, 42);
        filter.Received(1).Invoke(1, 42);
    }
    
    [Fact]
    public void SubscribeOnce_Stateful_WithActionHandlerAndPredicateFilterAndAdditionalFilters_Works() {
        var action = Substitute.For<Action<int, int>>();
        var filter = Substitute.For<Func<int, int, bool>>();
        filter.Invoke(1, 42).Returns(true);
        
        evt.SubscribeOnce(1, action, filter, filters);
        evt.Publish(42);
        evt.Publish(42);
        
        action.Received(1).Invoke(1, 42);
        filter.Received(1).Invoke(1, 42);
        foreach (var f in filters) {
            f.Received(1).Filter(ref Arg.Any<int>());
        }
    }

    // ===== Void Overloads ===== //
    [Fact]
    public void SubscribeOnce_Void_WithHandler_Works() {
        var action = Substitute.For<Action>();
        
        voidEvt.SubscribeOnce(action);
        voidEvt.Publish(Void.Value);
        voidEvt.Publish(Void.Value);
        
        action.Received(1).Invoke();
    }
    
    [Fact]
    public void SubscribeOnce_Void_WithHandlerAndFilters_Works() {
        var action = Substitute.For<Action>();
        
        voidEvt.SubscribeOnce(action, voidFilters);
        voidEvt.Publish(Void.Value);
        voidEvt.Publish(Void.Value);
        
        action.Received(1).Invoke();
        foreach (var filter in voidFilters) {
            filter.Received(1).Filter(ref Arg.Any<Void>());
        }
    }

    [Fact]
    public void SubscribeOnce_Void_WithHandlerAndPredicateFilter_Works() {
        var action = Substitute.For<Action>();
        var predicateFilter = Substitute.For<Func<bool>>();
        predicateFilter.Invoke().Returns(true);
        
        voidEvt.SubscribeOnce(action, predicateFilter);
        voidEvt.Publish(Void.Value);
        voidEvt.Publish(Void.Value);

        action.Received(1).Invoke();
        predicateFilter.Received(1).Invoke();
    }
    
    [Fact]
    public void SubscribeOnce_Void_WithHandlerAndPredicateFilterAndAdditionalFilters_Works() {
        var action = Substitute.For<Action>();
        var predicateFilter = Substitute.For<Func<bool>>();
        predicateFilter.Invoke().Returns(true);
        
        voidEvt.SubscribeOnce(action, predicateFilter, voidFilters);
        voidEvt.Publish(Void.Value);
        voidEvt.Publish(Void.Value);

        action.Received(1).Invoke();
        predicateFilter.Received(1).Invoke();
        foreach (var filter in voidFilters) {
            filter.Received(1).Filter(ref Arg.Any<Void>());
        }
    }
    
    // ===== Stateful Void Overloads ===== //
    [Fact]
    public void SubscribeOnce_Void_Stateful_WithHandler_Works() {
        var action = Substitute.For<Action<int>>();
        
        voidEvt.SubscribeOnce(1, action);
        voidEvt.Publish(Void.Value);
        voidEvt.Publish(Void.Value);
        
        action.Received(1).Invoke(1);
    }
    
    [Fact]
    public void SubscribeOnce_Void_Stateful_WithHandlerFilters_Works() {
        var action = Substitute.For<Action<int>>();
        
        voidEvt.SubscribeOnce(1, action, voidFilters);
        voidEvt.Publish(Void.Value);
        voidEvt.Publish(Void.Value);
        
        action.Received(1).Invoke(1);
        foreach (var filter in voidFilters) {
            filter.Received(1).Filter(ref Arg.Any<Void>());
        }   
    }

    [Fact]
    public void SubscribeOnce_Void_Stateful_WithHandlerAndPredicateFilter_Works() {
        var action = Substitute.For<Action<int>>();
        var predicateFilter = Substitute.For<Func<int, bool>>();
        predicateFilter.Invoke(1).Returns(true);

        voidEvt.SubscribeOnce(1, action, predicateFilter);
        voidEvt.Publish(Void.Value);
        voidEvt.Publish(Void.Value);

        action.Received(1).Invoke(1);
        predicateFilter.Received(1).Invoke(1);
    }

    [Fact]
    public void SubscribeOnce_Void_Stateful_WithHandlerAndPredicateFilterAndAdditionalFilters_Works() {
        var action = Substitute.For<Action<int>>();
        var predicateFilter = Substitute.For<Func<int, bool>>();
        predicateFilter.Invoke(1).Returns(true);

        voidEvt.SubscribeOnce(1, action, predicateFilter, voidFilters);
        voidEvt.Publish(Void.Value);
        voidEvt.Publish(Void.Value);

        action.Received(1).Invoke(1);
        predicateFilter.Received(1).Invoke(1);
        foreach (var filter in voidFilters) {
            filter.Received(1).Filter(ref Arg.Any<Void>());
        }
    }
    
    // ===== Edge Case Tests ===== //
    [Fact]
    public void SubscribeOnce_WhenFilterRejects_HandlerNotInvokedAndSubscriptionRemainsActive() {
        var action = Substitute.For<Action<int>>();
        var filter = Substitute.For<IEventFilter<int>>();
        
        // First event is rejected
        filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);
        
        evt.SubscribeOnce(action, filter);
        evt.Publish(10);
        
        // Handler should not have been invoked yet
        action.DidNotReceive().Invoke(Arg.Any<int>());
        
        // Second event passes the filter
        filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        
        evt.Publish(20);
        
        // Handler should now be invoked exactly once
        action.Received(1).Invoke(20);
        
        // Third event should not invoke handler (already unsubscribed)
        evt.Publish(30);
        action.Received(1).Invoke(Arg.Any<int>());
    }
    
    [Fact]
    public void SubscribeOnce_ManualDisposalBeforeFirstEvent_HandlerNeverInvoked() {
        var action = Substitute.For<Action<int>>();
        
        var subscription = evt.SubscribeOnce(action);
        subscription.Dispose();
        
        evt.Publish(42);
        evt.Publish(43);
        
        // Handler should never be invoked after disposal
        action.DidNotReceive().Invoke(Arg.Any<int>());
    }
    
    [Fact]
    public void SubscribeOnce_HandlerThrowsException_SubscriptionStillDisposed() {
        var action = Substitute.For<Action<int>>();
        action.When(x => x.Invoke(Arg.Any<int>())).Do(_ => throw new InvalidOperationException("Test exception"));
        
        evt.SubscribeOnce(action);
        
        // Handler throws, but should not crash the test
        Assert.Throws<InvalidOperationException>(() => evt.Publish(42));
        
        // Even though handler threw, subscription should be disposed
        // Second event should not invoke handler again
        action.ClearReceivedCalls();
        evt.Publish(43);
        
        action.DidNotReceive().Invoke(Arg.Any<int>());
    }
}