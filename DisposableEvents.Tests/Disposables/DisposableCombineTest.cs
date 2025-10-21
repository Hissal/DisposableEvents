using DisposableEvents.Disposables;

namespace DisposableEvents.Tests.Disposables;

[TestSubject(typeof(Disposable))]
public class DisposableCombineTest {
    readonly IDisposable disposable1;
    readonly IDisposable disposable2;
    readonly IDisposable disposable3;
    readonly IDisposable disposable4;
    readonly IDisposable disposable5;
    readonly IDisposable disposable6;
    readonly IDisposable disposable7;
    readonly IDisposable disposable8;
    readonly IDisposable disposable9;
    
    public DisposableCombineTest() {
        disposable1 = Substitute.For<IDisposable>();
        disposable2 = Substitute.For<IDisposable>();
        disposable3 = Substitute.For<IDisposable>();
        disposable4 = Substitute.For<IDisposable>();
        disposable5 = Substitute.For<IDisposable>();
        disposable6 = Substitute.For<IDisposable>();
        disposable7 = Substitute.For<IDisposable>();
        disposable8 = Substitute.For<IDisposable>();
        disposable9 = Substitute.For<IDisposable>();
    }

    [Fact]
    public void Combine_Two_DisposesBoth() {
        var combined = Disposable.Combine(disposable1, disposable2);
        combined.Dispose();
        Assert.All([disposable1, disposable2], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Combine_Three_DisposesAll() {
        var combined = Disposable.Combine(disposable1, disposable2, disposable3);
        combined.Dispose();
        Assert.All([disposable1, disposable2, disposable3], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Combine_Four_DisposesAll() {
        var combined = Disposable.Combine(disposable1, disposable2, disposable3, disposable4);
        combined.Dispose();
        Assert.All([disposable1, disposable2, disposable3, disposable4], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Combine_Five_DisposesAll() {
        var combined = Disposable.Combine(disposable1, disposable2, disposable3, disposable4, disposable5);
        combined.Dispose();
        Assert.All([disposable1, disposable2, disposable3, disposable4, disposable5], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Combine_Six_DisposesAll() {
        var combined = Disposable.Combine(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6);
        combined.Dispose();
        Assert.All([disposable1, disposable2, disposable3, disposable4, disposable5, disposable6], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Combine_Seven_DisposesAll() {
        var combined = Disposable.Combine(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7);
        combined.Dispose();
        Assert.All([disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Combine_Eight_DisposesAll() {
        var combined = Disposable.Combine(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7, disposable8);
        combined.Dispose();
        Assert.All([disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7, disposable8
        ], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Combine_Nine_DisposesAll_ViaParams() {
        var combined = Disposable.Combine(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7, disposable8, disposable9);
        combined.Dispose();
        Assert.All([disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7, disposable8, disposable9
        ], d => d.Received(1).Dispose());
    }
}