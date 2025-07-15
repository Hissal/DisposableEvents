using DisposableEvents.EventContainers;

namespace UnitTests.EventContainers;

[TestFixture]
public class UnkeyedEventContainerTests {
    [Test]
    public void RegisterEvent_CreatesAndReturnsEvent() {
        var container = new UnkeyedEventContainer();
        var evt = container.RegisterEvent<int>();
        Assert.That(evt, Is.Not.Null);
    }

    [Test]
    public void RegisterEvent_SameTypeTwice_Throws() {
        var container = new UnkeyedEventContainer();
        container.RegisterEvent<int>();
        Assert.Throws<InvalidOperationException>(() => container.RegisterEvent<int>());
    }

    [Test]
    public void TryGetEvent_ReturnsFalseIfNotRegistered() {
        var container = new UnkeyedEventContainer();
        
        var found = container.TryGetEvent<int>(out var evt);
        
        Assert.Multiple(() => {
            Assert.That(found, Is.False);
            Assert.That(evt, Is.Null);
        });
    }

    [Test]
    public void TryGetEvent_ReturnsTrueIfRegistered() {
        var container = new UnkeyedEventContainer();
        
        var registered = container.RegisterEvent<int>();
        var found = container.TryGetEvent<int>(out var evt);
        
        Assert.Multiple(() => {
            Assert.That(found, Is.True);
            Assert.That(evt, Is.SameAs(registered));
        });
    }
    
    [Test]
    public void RegisterAndPublish_PublishesEvent() {
        var container = new UnkeyedEventContainer();
        var evt = new TestEvent<int>();
        
        container.RegisterEvent(evt);
        container.Publish(42);
        
        Assert.That(evt.LastPublishedValue, Is.EqualTo(42));
    }

    [Test]
    public void SubscribeAndPublishes_PublishesToObserver() {
        var container = new UnkeyedEventContainer();
        var obs = new TestObserver<int>();
        
        container.Subscribe(obs);
        container.Publish(42);
        
        Assert.That(obs.LastValue, Is.EqualTo(42));
    }

    [Test]
    public void Publish_NoSubscribers_DoesNotThrow() {
        var container = new UnkeyedEventContainer();
        Assert.DoesNotThrow(() => container.Publish(123));
    }

    [Test]
    public void Dispose_DisposesAllEvents() {
        var container = new UnkeyedEventContainer();
        var evt1 = new TestEvent<int>();
        var evt2 = new TestEvent<string>();
        
        container.RegisterEvent(evt1);
        container.RegisterEvent(evt2);

        container.Dispose();

        Assert.Multiple(() => {
            Assert.That(evt1.IsDisposed, Is.True);
            Assert.That(evt2.IsDisposed, Is.True);
        });
    }
}