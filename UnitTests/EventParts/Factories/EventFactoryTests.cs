using DisposableEvents;
using DisposableEvents.Factories;

namespace UnitTests.EventParts.Factories;

[TestFixture]
public class EventFactoryTests {
    // Unkeyed Event Factory Tests
    [Test]
    public void Create_ReturnsEventInstance() {
        var factory = new EventFactory();

        var evt = factory.Create<int>();

        Assert.That(evt, Is.Not.Null);
        Assert.That(evt, Is.InstanceOf<Event<int>>());
    }

    [Test]
    public void Create_DifferentTypes_ReturnsDifferentEventInstances() {
        var factory = new EventFactory();

        var evt1 = factory.Create<int>();
        var evt2 = factory.Create<string>();

        Assert.That(evt1, Is.Not.SameAs(evt2));
        Assert.Multiple(() => {
            Assert.That(evt1, Is.InstanceOf<Event<int>>());
            Assert.That(evt2, Is.InstanceOf<Event<string>>());
        });
    }

    [Test]
    public void Default_ReturnsSingletonInstance() {
        var instance1 = EventFactory.Default;
        var instance2 = EventFactory.Default;

        Assert.That(instance1, Is.SameAs(instance2));
    }

    // Keyed Event Factory Tests
    [Test]
    public void CreateKeyed_ReturnsKeyedEventInstance() {
        var factory = new KeyedEventFactory();

        var evt = factory.Create<string, int>();

        Assert.That(evt, Is.Not.Null);
        Assert.That(evt, Is.InstanceOf<KeyedEvent<string, int>>());
    }

    [Test]
    public void CreateKeyed_DifferentTypes_ReturnsDifferentKeyedEventInstances() {
        var factory = new KeyedEventFactory();

        var evt1 = factory.Create<string, int>();
        var evt2 = factory.Create<int, string>();

        Assert.That(evt1, Is.Not.SameAs(evt2));
        Assert.Multiple(() => {
            Assert.That(evt1, Is.InstanceOf<KeyedEvent<string, int>>());
            Assert.That(evt2, Is.InstanceOf<KeyedEvent<int, string>>());
        });
    }

    [Test]
    public void KeyedEventFactory_Default_ReturnsSingletonInstance() {
        var instance1 = KeyedEventFactory.Default;
        var instance2 = KeyedEventFactory.Default;

        Assert.That(instance1, Is.SameAs(instance2));
    }
}