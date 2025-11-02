using DisposableEvents;
using DisposableEvents.Factories;

namespace DisposableEvents.Tests.Hub.Event;

[TestSubject(typeof(ManualEventHub<>))]
public class RestrictedManualEventHubTest {
    public interface ITest;
    public struct TestValue : ITest;
    public struct NonRegisteredMessage : ITest;
    
    readonly ManualEventHub<ITest> sut;
    readonly IDisposableEvent<TestValue> registeredEvent;
    
    public RestrictedManualEventHubTest() {
        registeredEvent = Substitute.For<IDisposableEvent<TestValue>>();
        sut = ManualEventHub<ITest>.CreateBuilder()
            .RegisterEvent(registeredEvent)
            .Build();
    }
    
    // ----- EventHub Tests ----- //
    [Fact]
    public void GetPublisher_ReturnsRegisteredEvent() {
        var publisher = sut.GetPublisher<TestValue>();
        publisher.Should().BeSameAs(registeredEvent);
    }

    [Fact]
    public void GetSubscriber_ReturnsRegisteredEvent() {
        var subscriber = sut.GetSubscriber<TestValue>();
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
        
        var publisher = sut.GetPublisher<TestValue>();
        var subscriber = sut.GetSubscriber<TestValue>();
        
        publisher.Should().BeSameAs(NullEvent<TestValue>.Instance);
        subscriber.Should().BeSameAs(NullEvent<TestValue>.Instance);
    }

    [Fact]
    public void TryGetEvent_ReturnsRegisteredEvent() {
        var result = sut.TryGetEvent<TestValue>(out var eventInstance);
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
        var customEvent = Substitute.For<IDisposableEvent<TestValue>>();
        var customSut = ManualEventHub.CreateBuilder()
            .RegisterEvent(customEvent)
            .Build();
        
        var publisher = customSut.GetPublisher<TestValue>();
        
        publisher.Should().BeSameAs(customEvent);
    }

    [Fact]
    public void Builder_RegisterEventNonInjected_RegistersFactoryProducedEvent() {
        // Arrange
        var factory = Substitute.For<IEventFactory>();
        var factoryProducedEvent = Substitute.For<IDisposableEvent<TestValue>>();
        factory.CreateEvent<TestValue>().Returns(factoryProducedEvent);
        
        var customSut = ManualEventHub.CreateBuilder()
            .SetFactory(factory)
            .RegisterEvent<TestValue>()
            .Build();
        
        // Act
        var publisher = customSut.GetPublisher<TestValue>();
        
        // Assert
        publisher.Should().BeSameAs(factoryProducedEvent);
    }
}