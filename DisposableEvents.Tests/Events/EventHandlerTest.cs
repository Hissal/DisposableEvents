using DisposableEvents;

namespace DisposableEvents.Tests.Events;

[TestSubject(typeof(EventHandler<>))]
[TestSubject(typeof(EventHandler<,>))]
public class EventHandlerTest {

    [Fact]
    public void EventHandler_InvokesAction() {
        var action = Substitute.For<Action<int>>();
        var sut = new EventHandler<int>(action);
        
        sut.Handle(69);
        
        action.Received(1).Invoke(69);
    }
    
    [Fact]
    public void StatefulEventHandler_InvokesActionWithState() {
        var action = Substitute.For<Action<string, int>>();
        var sut = new EventHandler<string, int>("state", action);
        
        sut.Handle(42);
        
        action.Received(1).Invoke("state", 42);
    }
}