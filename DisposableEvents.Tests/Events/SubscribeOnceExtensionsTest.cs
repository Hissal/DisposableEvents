using DisposableEvents;

namespace DisposableEvents.Tests.Events;

[TestSubject(typeof(SubscribeOnceExtensions))]
public class SubscribeOnceExtensionsTest {

    [Fact]
    public void SubscribeOnce_Works() {
        var disposableEvent = new DisposableEvent<int>();
        int callCount = 0;
        disposableEvent.SubscribeOnce(_ => callCount++);

        disposableEvent.Publish(1);
        disposableEvent.Publish(2);
        disposableEvent.Publish(3);

        Assert.Equal(1, callCount);
    }
}