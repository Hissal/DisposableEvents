using DisposableEvents.EventContainers;

namespace UnitTests.EventContainers;

[TestFixture]
public class EventContainerTests {
    TestUnkeyedEventContainer unkeyedContainer;
    TestKeyedEventContainer keyedContainer;
    EventContainer container;
    
    [SetUp]
    public void Setup() {
        unkeyedContainer = new TestUnkeyedEventContainer();
        keyedContainer = new TestKeyedEventContainer();
        container = new EventContainer(unkeyedContainer, keyedContainer);
    }

    [TearDown]
    public void TearDown() {
        unkeyedContainer.Dispose();
        keyedContainer.Dispose();
        container.Dispose();
    }
    
    // Unkeyed container events
    
    [Test]
    public void RegisterEvent_RegistersEventInUnkeyedContainer() {
        var evt = new TestEvent<int>();
       container.RegisterEvent(evt);
        Assert.That(unkeyedContainer.RegisteredEvents, Contains.Item(evt));
    }

    [Test]
    public void TryGetEvent_ReturnsFalseIfNotRegisteredUnkeyed() {
        var found = container.TryGetEvent<int>(out var evt);
        
        Assert.Multiple(() => {
            Assert.That(found, Is.False);
            Assert.That(evt, Is.Null);
        });
    }

    [Test]
    public void TryGetEvent_ReturnsTrueIfRegisteredUnkeyed() {
        var evt = new TestEvent<int>();
        container.RegisterEvent(evt);
        
        var found = container.TryGetEvent<int>(out var retrievedEvt);
        
        Assert.Multiple(() => {
            Assert.That(found, Is.True);
            Assert.That(retrievedEvt, Is.SameAs(evt));
        });
    }
    
    [Test]
    public void Subscribe_AddsSubscriberToUnkeyedContainer() {
        var obs = new TestObserver<int>();
        container.Subscribe(obs);
        Assert.That(unkeyedContainer.SubscribedObservers, Contains.Item(obs));
    }

    [Test]
    public void Publishes_PublishesToUnkeyedContainer() {
        container.Publish(123);
        Assert.That(unkeyedContainer.LastPublishedValue, Is.EqualTo(123));
    }
    
    // Keyed container events
    
    [Test]
    public void RegisterEvent_RegistersEventInKeyedContainer() {
        var evt = new TestKeyedEvent<int, int>();
        container.RegisterEvent(evt);
        Assert.That(keyedContainer.RegisteredEvents, Contains.Item(evt));
    }

    [Test]
    public void TryGetEvent_ReturnsFalseIfNotRegisteredKeyed() {
        var found = container.TryGetEvent<int, int>(out var evt);
        
        Assert.Multiple(() => {
            Assert.That(found, Is.False);
            Assert.That(evt, Is.Null);
        });
    }

    [Test]
    public void TryGetEvent_ReturnsTrueIfRegisteredKeyed() {
        var evt = new TestKeyedEvent<int, int>();
        container.RegisterEvent(evt);
        
        var found = container.TryGetEvent<int, int>(out var retrievedEvt);
        
        Assert.Multiple(() => {
            Assert.That(found, Is.True);
            Assert.That(retrievedEvt, Is.SameAs(evt));
        });
    }
    
    [Test]
    public void Subscribe_AddsSubscriberToKeyedContainer() {
        var obs = new TestObserver<int>();
        container.Subscribe(1, obs);
        Assert.That(keyedContainer.SubscribedObservers, Contains.Item(obs));
    }

    [Test]
    public void Publishes_PublishesToKeyedContainer() {
        container.Publish(1, 123);
        Assert.That(keyedContainer.LastPublishedValue, Is.EqualTo(123));
    }
    
    // Both containers
    
    [Test]
    public void Dispose_DisposesBothContainers() {
        container.Dispose();

        Assert.Multiple(() => {
            Assert.That(unkeyedContainer.IsDisposed, Is.True, "Unkeyed container should be disposed");
            Assert.That(keyedContainer.IsDisposed, Is.True, "Keyed container should be disposed");
        });
    }
}