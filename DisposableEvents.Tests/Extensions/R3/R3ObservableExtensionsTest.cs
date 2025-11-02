using DisposableEvents.R3;
using R3;

namespace DisposableEvents.Tests.Extensions;

[TestSubject(typeof(R3ObservableExtensions))]
public class R3ObservableExtensionsTest {
    readonly DisposableEvent<int> evt = new DisposableEvent<int>();
    readonly Observer<int> obs = Substitute.For<Observer<int>>();
    
    [Fact]
    public void AsR3Observable_Publishes_ThroughObserver() {
        evt.AsR3Observable().Subscribe(obs);
        evt.Publish(42);
        obs.Received(1).OnNext(42);
    }
    
    [Fact]
    public void AsR3ObservableWithFilter_Publishes_ThroughObserver() {
        var filter = Substitute.For<IEventFilter<int>>();
        filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        
        evt.AsR3Observable(filter).Subscribe(obs);
        evt.Publish(42);
        
        obs.Received(1).OnNext(42);
    }
    
    [Fact]
    public void AsR3ObservableWithMultipleFilters_Publishes_ThroughObserver() {
        var filter1 = Substitute.For<IEventFilter<int>>();
        var filter2 = Substitute.For<IEventFilter<int>>();
        filter1.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        filter2.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        
        evt.AsR3Observable(filter1, filter2).Subscribe(obs);
        evt.Publish(42);
        
        obs.Received(1).OnNext(42);
    }
    
    [Fact]
    public void AsR3ObservableWithFilter_BlocksMessage_WhenFilterBlocks() {
        var filter = Substitute.For<IEventFilter<int>>();
        filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);
        
        evt.AsR3Observable(filter).Subscribe(obs);
        evt.Publish(42);
        
        obs.DidNotReceive().OnNext(Arg.Any<int>());
    }
    
    [Fact]
    public void AsR3ObservableWithMultipleFilters_BlocksMessage_WhenAnyFilterBlocks() {
        var filter1 = Substitute.For<IEventFilter<int>>();
        var filter2 = Substitute.For<IEventFilter<int>>();
        filter1.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        filter2.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block); 
        
        evt.AsR3Observable(filter1, filter2).Subscribe(obs);
        evt.Publish(42);
        
        obs.DidNotReceive().OnNext(Arg.Any<int>());
    }
    
    [Fact]
    public void AsR3Observable_DisposeSubscription_DoesNotReceiveMessages() {
        var subscription = evt.AsR3Observable().Subscribe(obs);
        
        subscription.Dispose();
        evt.Publish(42);
        
        obs.DidNotReceive().OnNext(42);
    }
}