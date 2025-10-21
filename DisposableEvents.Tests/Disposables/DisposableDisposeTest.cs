using DisposableEvents.Disposables;

namespace DisposableEvents.Tests.Disposables;

[TestSubject(typeof(Disposable))]
public class DisposableDisposeTest {
    readonly IDisposable disposable1;
    readonly IDisposable disposable2;
    readonly IDisposable disposable3;
    readonly IDisposable disposable4;
    readonly IDisposable disposable5;
    readonly IDisposable disposable6;
    readonly IDisposable disposable7;
    readonly IDisposable disposable8;
    readonly IDisposable disposable9;
    
    public DisposableDisposeTest() {
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
    public void Dispose_Two_DisposesBoth() {
        Disposable.Dispose(disposable1, disposable2);
        Assert.All([disposable1, disposable2], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Dispose_Three_DisposesAll() {
        Disposable.Dispose(disposable1, disposable2, disposable3);
        Assert.All([disposable1, disposable2, disposable3], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Dispose_Four_DisposesAll() {
        Disposable.Dispose(disposable1, disposable2, disposable3, disposable4);
        Assert.All([disposable1, disposable2, disposable3, disposable4], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Dispose_Five_DisposesAll() {
        Disposable.Dispose(disposable1, disposable2, disposable3, disposable4, disposable5);
        Assert.All([disposable1, disposable2, disposable3, disposable4, disposable5], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Dispose_Six_DisposesAll() {
        Disposable.Dispose(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6);
        Assert.All([disposable1, disposable2, disposable3, disposable4, disposable5, disposable6], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Dispose_Seven_DisposesAll() {
        Disposable.Dispose(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7);
        Assert.All([disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Dispose_Eight_DisposesAll() {
        Disposable.Dispose(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7, disposable8);
        Assert.All([disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7, disposable8
        ], d => d.Received(1).Dispose());
    }

    [Fact]
    public void Dispose_Nine_DisposesAll_ViaParams() {
        Disposable.Dispose(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7, disposable8, disposable9);
        Assert.All([disposable1, disposable2, disposable3, disposable4, disposable5, disposable6, disposable7, disposable8, disposable9
        ], d => d.Received(1).Dispose());
    }
}