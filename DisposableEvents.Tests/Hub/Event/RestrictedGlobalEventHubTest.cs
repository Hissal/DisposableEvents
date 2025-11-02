namespace DisposableEvents.Tests.Hub.Event;

[TestSubject(typeof(GlobalEventHub<>))]
public class RestrictedGlobalEventHubTest {
    public interface ITest;
    public struct TestValue : ITest;
    
    readonly IEventHub<ITest> hub;
    public RestrictedGlobalEventHubTest() {
        hub = Substitute.For<IEventHub<ITest>>();
        GlobalEventHub<ITest>.SetHub(hub);
    }

    [Fact]
    public void GetPublisher_Returns_FactoryProducedEvent() {
        GlobalEventHub<ITest>.GetPublisher<TestValue>();
        hub.Received(1).GetPublisher<TestValue>();
    }

    [Fact]
    public void GetSubscriber_Returns_FactoryProducedEvent() {
        GlobalEventHub<ITest>.GetSubscriber<TestValue>();
        hub.Received(1).GetSubscriber<TestValue>();
    }
    
    [Fact]
    public void Throws_WhenHubIsNull() {
        GlobalEventHub<ITest>.SetHub(null!);
        
        Action act = () => GlobalEventHub<ITest>.GetPublisher<TestValue>();
        Action act2 = () => GlobalEventHub<ITest>.GetPublisher<TestValue>();
        
        act.Should().Throw<InvalidOperationException>();
        act2.Should().Throw<InvalidOperationException>();
    }
}