namespace DisposableEvents.Tests.Hub.Event;

[TestSubject(typeof(GlobalEventHub))]
public class GlobalEventHubTest {
    readonly IEventHub hub;
    public GlobalEventHubTest() {
        hub = Substitute.For<IEventHub>();
        GlobalEventHub.SetHub(hub);
    }

    [Fact]
    public void GetPublisher_Returns_FactoryProducedEvent() {
        GlobalEventHub.GetPublisher<int>();
        hub.Received(1).GetPublisher<int>();
    }

    [Fact]
    public void GetSubscriber_Returns_FactoryProducedEvent() {
        GlobalEventHub.GetSubscriber<int>();
        hub.Received(1).GetSubscriber<int>();
    }
    
    [Fact]
    public void Throws_WhenHubIsNull() {
        GlobalEventHub.SetHub(null!);
        
        Action act = () => GlobalEventHub.GetPublisher<int>();
        Action act2 = () => GlobalEventHub.GetPublisher<int>();
        
        act.Should().Throw<InvalidOperationException>();
        act2.Should().Throw<InvalidOperationException>();
    }
}