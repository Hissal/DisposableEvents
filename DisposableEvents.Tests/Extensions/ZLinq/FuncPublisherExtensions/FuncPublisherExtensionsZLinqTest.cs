using DisposableEvents.ZLinq;
using ZLinq;

namespace DisposableEvents.Tests.Extensions.ZLinq.FuncPublisherExtensions;

[TestSubject(typeof(FuncPublisherExtensionsZLinq))]
public class FuncPublisherExtensionsZLinqTest {
    readonly DisposableFunc<int, int> func;
    readonly IFuncHandler<int, int>[] handlers;
    readonly FuncResult<int>[] expectedResults;
    
    public FuncPublisherExtensionsZLinqTest() {
        func = new DisposableFunc<int, int>();
        handlers = Enumerable.Range(0, 10).Select(_ => Substitute.For<IFuncHandler<int, int>>()).ToArray();
        expectedResults = new FuncResult<int>[10];
        for (var i = 0; i < handlers.Length; i++) {
            var result = FuncResult<int>.From(i);
            handlers[i].Handle(Arg.Any<int>()).Returns(result);
            expectedResults[i] = result;
            func.RegisterHandler(handlers[i]);
        }
    }
    
    [Fact]
    public void InvokeAsValueEnumerable_ReturnsExpectedResults() {
        var results = func.InvokeAsValueEnumerable(42).ToArray();
        results.Should().Equal(expectedResults);
    }
    
    [Fact]
    public void InvokeAsValueEnumerable_InvocationsAreDeferred() {
        var results = func.InvokeAsValueEnumerable(42);
        
        foreach (var handler in handlers) {
            handler.DidNotReceive().Handle(Arg.Any<int>());
        }
        
        results.ToArray();
        
        foreach (var handler in handlers) {
            handler.Received(1).Handle(42);
        }
    }
    
    [Fact]
    public void InvokeAsValueEnumerableImmediate_ReturnsExpectedResults() {
        var results = func.InvokeAsValueEnumerableImmediate(42).ToArray();
        results.Should().Equal(expectedResults);
    }

    [Fact]
    public void InvokeAsValueEnumerableImmediate_InvokesImmediately() {
        func.InvokeAsValueEnumerableImmediate(42);
        foreach (var handler in handlers) {
            handler.Received(1).Handle(42);
        }
    }
}