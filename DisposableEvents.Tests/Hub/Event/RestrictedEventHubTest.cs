using DisposableEvents;
using DisposableEvents.Factories;

namespace DisposableEvents.Tests.Hub.Event;

[TestSubject(typeof(EventHub<>))]
public class RestrictedEventHubTest {
    public interface ITest;
    public struct TestValue : ITest;
    
    readonly EventHub<ITest> sut;
    readonly IDisposableEvent<TestValue> producedEvent;
    
    public RestrictedEventHubTest() {
        var factory = Substitute.For<IEventFactory>();
        producedEvent = Substitute.For<IDisposableEvent<TestValue>>();
        factory.CreateEvent<TestValue>().Returns(producedEvent);
        sut = new EventHub<ITest>(factory);
    }
    
    [Fact]
    public void GetPublisher_Returns_FactoryProducedEvent() {
        var publisher = sut.GetPublisher<TestValue>();
        publisher.Should().BeSameAs(producedEvent);
    }

    [Fact]
    public void GetSubscriber_Returns_FactoryProducedEvent() {
        var subscriber = sut.GetSubscriber<TestValue>();
        subscriber.Should().BeSameAs(producedEvent);
    }
    
    [Fact]
    public void Dispose_DisposesRegistry() {
        sut.GetPublisher<TestValue>(); // Ensure event is created
        sut.Dispose();
        producedEvent.Received(1).Dispose();
    }
    
    [Fact]
    public void Returns_NullEvent_AfterDispose() {
        sut.Dispose();
        
        var publisher = sut.GetPublisher<TestValue>();
        var subscriber = sut.GetSubscriber<TestValue>();
        
        publisher.Should().BeSameAs(NullEvent<TestValue>.Instance);
        subscriber.Should().BeSameAs(NullEvent<TestValue>.Instance);
    }
}