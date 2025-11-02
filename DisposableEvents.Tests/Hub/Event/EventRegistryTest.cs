namespace DisposableEvents.Tests.Hub.Event;

[TestSubject(typeof(EventRegistry))]
public class EventRegistryTest {
    readonly EventRegistry sut;
    
    public EventRegistryTest() {
        sut = new EventRegistry();
    }

    // ----- Register ----- //
    [Fact]
    public void RegisterEvent_ForNewEvent_ReturnsSuccess() {
        var disposableEvent = Substitute.For<IDisposableEvent<int>>();
        var result = sut.RegisterEvent(disposableEvent);
        result.Should().Be(EventRegistrationResult.Success);
    }

    [Fact]
    public void RegisterEvent_ForAlreadyRegisteredEvent_ReturnsAlreadyRegistered() {
        var disposableEvent = Substitute.For<IDisposableEvent<int>>();
        
        sut.RegisterEvent(disposableEvent);
        var result = sut.RegisterEvent(disposableEvent);
        
        result.Should().Be(EventRegistrationResult.AlreadyRegistered);
    }
    
    // ----- Get ----- //
    [Fact]
    public void GetEvent_ForRegisteredEvent_ReturnsEvent() {
        var disposableEvent = Substitute.For<IDisposableEvent<int>>();
       
        sut.RegisterEvent(disposableEvent);
        var retrievedEvent = sut.GetEvent<int>();
        
        retrievedEvent.Should().BeSameAs(disposableEvent);
    }
    
    [Fact]
    public void GetEvent_ForUnregisteredEvent_ThrowsKeyNotFoundException() {
        Action act = () => sut.GetEvent<int>();
        act.Should().Throw<KeyNotFoundException>();
    }

    // ----- TryGet ----- //
    [Fact]
    public void TryGetEvent_ForRegisteredEvent_ReturnsTrueAndEvent() {
        var disposableEvent = Substitute.For<IDisposableEvent<int>>();
       
        sut.RegisterEvent(disposableEvent);
        var result = sut.TryGetEvent<int>(out var retrievedEvent);
        
        result.Should().BeTrue();
        retrievedEvent.Should().BeSameAs(disposableEvent);
    }

    [Fact]
    public void TryGetEvent_ForUnregisteredEvent_ReturnsFalse() {
        var result = sut.TryGetEvent<int>(out var retrievedEvent);
        result.Should().BeFalse();
        retrievedEvent.Should().BeNull();
    }
    
    // ----- GetOrAdd ----- //
    [Fact]
    public void GetOrAddEvent_ForRegisteredEvent_ReturnsExistingEvent() {
        var disposableEvent = Substitute.For<IDisposableEvent<int>>();
        sut.RegisterEvent(disposableEvent);
        
        var retrievedEvent = sut.GetOrAddEvent(() => Substitute.For<IDisposableEvent<int>>());
        retrievedEvent.Should().BeSameAs(disposableEvent);
    }
    
    [Fact]
    public void GetOrAddEvent_ForUnregisteredEvent_AddsAndReturnsEvent() {
        var disposableEvent = Substitute.For<IDisposableEvent<int>>();
        // ReSharper disable once HeapView.CanAvoidClosure
        var retrievedEvent = sut.GetOrAddEvent(() => disposableEvent);
        retrievedEvent.Should().BeSameAs(disposableEvent);
    }
    
    [Fact]
    public void GetOrAddEventWithState_ForRegisteredEvent_ReturnsExistingEvent() {
        var disposableEvent = Substitute.For<IDisposableEvent<int>>();
        sut.RegisterEvent(disposableEvent);
        
        var retrievedEvent = sut.GetOrAddEvent(Substitute.For<IDisposableEvent<int>>(), evt => evt);
        retrievedEvent.Should().BeSameAs(disposableEvent);
    }
    
    [Fact]
    public void GetOrAddEventWithStatee_ForUnregisteredEvent_AddsAndReturnsEvent() {
        var disposableEvent = Substitute.For<IDisposableEvent<int>>();
        var retrievedEvent = sut.GetOrAddEvent(disposableEvent, evt => evt);
        retrievedEvent.Should().BeSameAs(disposableEvent);
    }
    
    // ----- Containes ----- //
    [Fact]
    public void ContainsEvent_ForRegisteredEvent_ReturnsTrue() {
        var disposableEvent = Substitute.For<IDisposableEvent<int>>();
        
        sut.RegisterEvent(disposableEvent);
        var result = sut.ContainsEvent<int>();
        
        result.Should().BeTrue();
    }

    // ----- Dispose ----- //
    [Fact]
    public void Dispose_DisposesAllRegisteredEvents() {
        var disposableEvent1 = Substitute.For<IDisposableEvent<int>>();
        var disposableEvent2 = Substitute.For<IDisposableEvent<string>>();
        
        sut.RegisterEvent(disposableEvent1);
        sut.RegisterEvent(disposableEvent2);
        sut.Dispose();
        
        disposableEvent1.Received(1).Dispose();
        disposableEvent2.Received(1).Dispose();
    }
}