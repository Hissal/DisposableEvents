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
    public async Task Handle_ConcurrentInvocations_InvokeHandlerOnlyOnce() {
        // Arrange
        var invokeCount = 0;
        var handler = new EventHandler<int>(_ => Interlocked.Increment(ref invokeCount));
        var oneShotHandler = new OneShotEventHandler<int>(handler);
        var subscriptionSub = Substitute.For<IDisposable>();
        oneShotHandler.SetSubscription(subscriptionSub);
        
        // Act - Launch 100 concurrent Handle calls
        var tasks = new Task[100];
        for (int i = 0; i < tasks.Length; i++) {
            var message = i;
            tasks[i] = Task.Run(() => oneShotHandler.Handle(message));
        }
        await Task.WhenAll(tasks);
        
        // Assert
        invokeCount.Should().Be(1, "handler should be invoked exactly once despite concurrent calls");
        subscriptionSub.Received(1).Dispose();
    }
    
    [Fact]
    public async Task SetSubscription_ConcurrentInvocations_DisposesAllButFirst() {
        // Arrange
        var oneShotHandler = new OneShotEventHandler<int>(innerHandlerSub);
        var subscriptions = Enumerable.Range(0, 100)
            .Select(_ => Substitute.For<IDisposable>())
            .ToArray();
        
        // Act - Launch 100 concurrent SetSubscription calls
        var tasks = new Task[100];
        for (int i = 0; i < tasks.Length; i++) {
            var index = i;
            tasks[i] = Task.Run(() => oneShotHandler.SetSubscription(subscriptions[index]));
        }
        await Task.WhenAll(tasks);
        
        // Assert - Exactly one subscription should be kept, all others should be disposed
        var disposedCount = subscriptions.Count(s => s.ReceivedCalls().Any(c => c.GetMethodInfo().Name == "Dispose"));
        disposedCount.Should().Be(99, "all subscriptions except the first one should be disposed");
        
        // Trigger the handler to verify the one subscription that was kept gets disposed
        oneShotHandler.Handle(42);
        var totalDisposedAfterHandle = subscriptions.Count(s => s.ReceivedCalls().Any(c => c.GetMethodInfo().Name == "Dispose"));
        totalDisposedAfterHandle.Should().Be(100, "all subscriptions should eventually be disposed");
    }
    
    [Fact]
    public async Task Handle_And_SetSubscription_ConcurrentRace_EnsuresNoDoubleFree() {
        // Arrange - Test the race between Handle being called and SetSubscription being called
        var invokeCount = 0;
        var handler = new EventHandler<int>(_ => {
            Interlocked.Increment(ref invokeCount);
            // Add a small delay to increase chance of race condition
            Thread.Sleep(1);
        });
        var oneShotHandler = new OneShotEventHandler<int>(handler);
        var subscriptionSub = Substitute.For<IDisposable>();
        
        // Act - Launch Handle and SetSubscription concurrently
        var handleTask = Task.Run(() => oneShotHandler.Handle(42));
        var setSubTask = Task.Run(() => oneShotHandler.SetSubscription(subscriptionSub));
        
        await Task.WhenAll(handleTask, setSubTask);
        
        // Assert - Handler invoked once and subscription disposed exactly once (no double-dispose)
        invokeCount.Should().Be(1, "handler should be invoked exactly once");
        subscriptionSub.Received(1).Dispose();
    }
    
    [Fact]
    public async Task Handle_And_SetSubscription_RepeatedRaces_EnsuresCorrectBehavior() {
        // Arrange - Repeat the race test many times to catch intermittent issues
        for (int iteration = 0; iteration < 50; iteration++) {
            var invokeCount = 0;
            var handler = new EventHandler<int>(_ => Interlocked.Increment(ref invokeCount));
            var oneShotHandler = new OneShotEventHandler<int>(handler);
            var subscriptionSub = Substitute.For<IDisposable>();
            
            // Act - Randomly order Handle and SetSubscription calls
            var tasks = new List<Task>();
            if (iteration % 2 == 0) {
                tasks.Add(Task.Run(() => oneShotHandler.Handle(42)));
                tasks.Add(Task.Run(() => oneShotHandler.SetSubscription(subscriptionSub)));
            } else {
                tasks.Add(Task.Run(() => oneShotHandler.SetSubscription(subscriptionSub)));
                tasks.Add(Task.Run(() => oneShotHandler.Handle(42)));
            }
            
            await Task.WhenAll(tasks.ToArray());
            
            // Assert
            invokeCount.Should().Be(1, $"iteration {iteration}: handler should be invoked exactly once");
            subscriptionSub.Received(1).Dispose();
        }
    }
}