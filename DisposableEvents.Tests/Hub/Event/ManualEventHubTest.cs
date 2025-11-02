using DisposableEvents.Factories;

namespace DisposableEvents.Tests.Hub.Event;

[TestSubject(typeof(ManualEventHub))]
public class ManualEventHubTest {
    public struct NonRegisteredMessage;
    
    readonly ManualEventHub sut;
    readonly IDisposableEvent<int> registeredEvent;
    
    public ManualEventHubTest() {
        registeredEvent = Substitute.For<IDisposableEvent<int>>();
        sut = ManualEventHub.CreateBuilder()
            .RegisterEvent(registeredEvent)
            .Build();
    }
    
    // ----- EventHub Tests ----- //
    
    [Fact]
    public void GetPublisher_ReturnsRegisteredEvent() {
        var publisher = sut.GetPublisher<int>();
        publisher.Should().BeSameAs(registeredEvent);
    }

    [Fact]
    public void GetSubscriber_ReturnsRegisteredEvent() {
        var subscriber = sut.GetSubscriber<int>();
        subscriber.Should().BeSameAs(registeredEvent);
    }
    
    [Fact]
    public void Dispose_DisposesRegistry() {
        sut.Dispose();
        registeredEvent.Received(1).Dispose();
    }
    
    [Fact]
    public void Returns_NullEvent_AfterDispose() {
        sut.Dispose();
        
        var publisher = sut.GetPublisher<int>();
        var subscriber = sut.GetSubscriber<int>();
        
        publisher.Should().BeSameAs(NullEvent<int>.Instance);
        subscriber.Should().BeSameAs(NullEvent<int>.Instance);
    }

    [Fact]
    public void TryGetEvent_ReturnsRegisteredEvent() {
        var result = sut.TryGetEvent<int>(out var eventInstance);
        result.Should().BeTrue();
        eventInstance.Should().BeSameAs(registeredEvent);
    }

    [Fact]
    public void TryGetEvent_ForUnregisteredEvent_ReturnsFalse() {
        var result = sut.TryGetEvent<NonRegisteredMessage>(out var eventInstance);
        result.Should().BeFalse();
        eventInstance.Should().BeNull();
    }

    [Fact]
    public void GetPublisher_ForUnregisteredEvent_ThrowsKeyNotFoundException() {
        Action action = () => sut.GetPublisher<NonRegisteredMessage>();
        action.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void GetSubscriber_ForUnregisteredEvent_ThrowsKeyNotFoundException() {
        Action action = () => sut.GetSubscriber<NonRegisteredMessage>();
        action.Should().Throw<KeyNotFoundException>();
    }
    
    // ----- Builder Tests ----- //
    [Fact]
    public void Builder_RegisterEvent_AddsEventToHub() {
        var customEvent = Substitute.For<IDisposableEvent<int>>();
        var customSut = ManualEventHub.CreateBuilder()
            .RegisterEvent(customEvent)
            .Build();
        
        var publisher = customSut.GetPublisher<int>();
        
        publisher.Should().BeSameAs(customEvent);
    }

    [Fact]
    public void Builder_RegisterEventNonInjected_RegistersFactoryProducedEvent() {
        // Arrange
        var factory = Substitute.For<IEventFactory>();
        var factoryProducedEvent = Substitute.For<IDisposableEvent<int>>();
        factory.CreateEvent<int>().Returns(factoryProducedEvent);
        
        var customSut = ManualEventHub.CreateBuilder()
            .SetFactory(factory)
            .RegisterEvent<int>()
            .Build();
        
        // Act
        var publisher = customSut.GetPublisher<int>();
        
        // Assert
        publisher.Should().BeSameAs(factoryProducedEvent);
    }
}