using DisposableEvents.Factories;

namespace DisposableEvents.Tests.Hub.Event;

[TestSubject(typeof(EventHub))]
public class EventHubTest {
    readonly EventHub sut;
    readonly IDisposableEvent<int> producedEvent;
    
    public EventHubTest() {
        var factory = Substitute.For<IEventFactory>();
        producedEvent = Substitute.For<IDisposableEvent<int>>();
        factory.CreateEvent<int>().Returns(producedEvent);
        sut = new EventHub(factory);
    }
    
    [Fact]
    public void GetPublisher_Returns_FactoryProducedEvent() {
        var publisher = sut.GetPublisher<int>();
        publisher.Should().BeSameAs(producedEvent);
    }

    [Fact]
    public void GetSubscriber_Returns_FactoryProducedEvent() {
        var subscriber = sut.GetSubscriber<int>();
        subscriber.Should().BeSameAs(producedEvent);
    }
    
    [Fact]
    public void Dispose_DisposesRegistry() {
        sut.GetPublisher<int>(); // Ensure event is created
        sut.Dispose();
        producedEvent.Received(1).Dispose();
    }
    
    [Fact]
    public void Returns_NullEvent_AfterDispose() {
        sut.Dispose();
        
        var publisher = sut.GetPublisher<int>();
        var subscriber = sut.GetSubscriber<int>();
        
        publisher.Should().BeSameAs(NullEvent<int>.Instance);
        subscriber.Should().BeSameAs(NullEvent<int>.Instance);
    }
}