using DisposableEvents.EventContainers;

namespace UnitTests.EventContainers;

[TestFixture]
public class KeyedEventContainerTests {
    [Test]
    public void RegisterEvent_CreatesAndReturnsKeyedEvent() {
        var container = new KeyedEventContainer();
        var evt = container.RegisterEvent<string, int>();
        Assert.That(evt, Is.Not.Null);
    }

    [Test]
    public void RegisterEvent_SameTypeTwice_Throws() {
        var container = new KeyedEventContainer();
        container.RegisterEvent<string, int>();
        Assert.Throws<InvalidOperationException>(() => container.RegisterEvent<string, int>());
    }

    [Test]
    public void TryGetEvent_ReturnsFalseIfNotRegistered() {
        var container = new KeyedEventContainer();

        var found = container.TryGetEvent<string, int>(out var evt);

        Assert.Multiple(() => {
            Assert.That(found, Is.False);
            Assert.That(evt, Is.Null);
        });
    }

    [Test]
    public void TryGetEvent_ReturnsTrueIfRegistered() {
        var container = new KeyedEventContainer();
        var registered = container.RegisterEvent<string, int>();

        var found = container.TryGetEvent<string, int>(out var evt);

        Assert.Multiple(() => {
            Assert.That(found, Is.True);
            Assert.That(evt, Is.SameAs(registered));
        });
    }

    [Test]
    public void RegisterAndPublish_PublishesEvent() {
        var container = new KeyedEventContainer();
        var evt = new TestKeyedEvent<string, int>();

        container.RegisterEvent(evt);
        container.Publish("key", 42);
        
        Assert.Multiple(() => {
            Assert.That(evt.LastPublishedKey, Is.EqualTo("key"));
            Assert.That(evt.LastPublishedValue, Is.EqualTo(42));
        });
    }

    [Test]
    public void SubscribeAndPublishes_PublishesToObserver() {
        var container = new KeyedEventContainer();
        var obs = new TestObserver<int>();
        
        container.Subscribe("key", obs);
        container.Publish("key", 42);
        
        Assert.That(obs.LastValue, Is.EqualTo(42));
    }

    [Test]
    public void Publish_NoSubscribers_DoesNotThrow() {
        var container = new KeyedEventContainer();
        Assert.DoesNotThrow(() => container.Publish("key", 123));
    }

    [Test]
    public void Dispose_DisposesAllEvents() {
        var container = new KeyedEventContainer();
        var evt1 = new TestKeyedEvent<string, int>();
        var evt2 = new TestKeyedEvent<string, string>();
        
        container.RegisterEvent(evt1);
        container.RegisterEvent(evt2);
        container.Dispose();
        
        Assert.Multiple(() => {
            Assert.That(evt1.IsDisposed, Is.True);
            Assert.That(evt2.IsDisposed, Is.True);
        });
    }
}