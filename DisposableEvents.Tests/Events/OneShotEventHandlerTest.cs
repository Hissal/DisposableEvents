using DisposableEvents;

namespace DisposableEvents.Tests.Events;

[TestSubject(typeof(OneShotEventHandler<>))]
public class OneShotEventHandlerTest {
    readonly OneShotEventHandler<int> sut;
    readonly IEventHandler<int> innerHandlerSub;
    
    public OneShotEventHandlerTest() {
        innerHandlerSub = Substitute.For<IEventHandler<int>>();
        sut = new OneShotEventHandler<int>(innerHandlerSub);
    }
    
    [Fact]
    public void Handler_IsInvokedOnlyOnce() {
        sut.Handle(1);
        sut.Handle(2);
        sut.Handle(3);
        
        innerHandlerSub.Received(1).Handle(1);
        innerHandlerSub.DidNotReceive().Handle(2);
        innerHandlerSub.DidNotReceive().Handle(3);
    }
    
    [Fact]
    public void Subscription_IsDisposedAfterHandle() {
        var subscriptionSub = Substitute.For<IDisposable>();
        sut.SetSubscription(subscriptionSub);
        
        sut.Handle(42);
        
        subscriptionSub.Received(1).Dispose();
    }
    
    [Fact]
    public void Subscription_IsDisposedImmediately_IfAlreadyInvoked() {
        sut.Handle(99);
        
        var subscriptionSub = Substitute.For<IDisposable>();
        sut.SetSubscription(subscriptionSub);
        
        subscriptionSub.Received(1).Dispose();
    }
    
    [Fact]
    public void SecondSubscription_IsDisposedImmediately_WhenSetSubscriptionCalledTwice() {
        var firstSubscriptionSub = Substitute.For<IDisposable>();
        var secondSubscriptionSub = Substitute.For<IDisposable>();
        
        sut.SetSubscription(firstSubscriptionSub);
        sut.SetSubscription(secondSubscriptionSub);
        
        // First subscription should not be disposed yet
        firstSubscriptionSub.DidNotReceive().Dispose();
        // Second subscription should be disposed immediately
        secondSubscriptionSub.Received(1).Dispose();
    }
}