using DisposableEvents;

namespace DisposableEvents.Tests.Events;

[TestSubject(typeof(VoidEventHandler))]
[TestSubject(typeof(VoidEventHandler<>))]
public class VoidEventHandlerTest {
    [Fact]
    public void EventHandler_InvokesAction() {
        var action = Substitute.For<Action>();
        var sut = new VoidEventHandler(action);
        
        sut.Handle(Void.Value);
        
        action.Received(1).Invoke();
    }

    [Fact]
    public void StateFulEventHandler_InvokesActionWithState() {
        var action = Substitute.For<Action<int>>();
        var sut = new VoidEventHandler<int>(42, action);

        sut.Handle(Void.Value);

        action.Received(1).Invoke(42);
    }
}