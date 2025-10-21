using DisposableEvents.Disposables;

namespace DisposableEvents.Tests.Disposables;

[TestSubject(typeof(DisposableBuilder))]
public class DisposableBuilderTest {
    [Fact]
    public void Build_NoItems_ReturnsEmpty() {
        var builder = new DisposableBuilder();
        var result = builder.Build();
        Assert.Same(Disposable.Empty, result);
    }

    [Fact]
    public void Build_SingleItem_ReturnsSameInstance() {
        // Arrange
        var builder = new DisposableBuilder();
        var d1 = Substitute.For<IDisposable>();
        builder.Add(d1);

        // Act
        var result = builder.Build();

        // Assert
        Assert.Same(d1, result);
    }

    [Fact]
    public void Build_TwoItems_DisposesBothWhenResultDisposed() {
        // Arrange
        var builder = new DisposableBuilder();
        var d1 = Substitute.For<IDisposable>();
        var d2 = Substitute.For<IDisposable>();
        builder.Add(d1);
        builder.Add(d2);

        // Act
        var combined = builder.Build();
        combined.Dispose();

        // Assert
        Assert.All([d1, d2], d => d.Received(1).Dispose());
        Assert.NotSame(d1, combined);
        Assert.NotSame(d2, combined);
    }

    [Fact]
    public void Build_MoreThanEightItems_DisposesAllOnCombinedDispose() {
        // Arrange
        var builder = new DisposableBuilder();
        var items = Enumerable.Range(0, 12).Select(_ => Substitute.For<IDisposable>()).ToArray();
        foreach (var d in items) builder.Add(d);

        // Act
        var combined = builder.Build();
        combined.Dispose();

        // Assert
        Assert.All(items, d => d.Received(1).Dispose());
    }

    [Fact]
    public void Add_AfterDispose_DisposesImmediately() {
        // Arrange
        var builder = new DisposableBuilder();
        var d = Substitute.For<IDisposable>();

        // Act
        builder.Dispose();
        builder.Add(d);

        // Assert
        d.Received(1).Dispose();
    }

    [Fact]
    public void Build_Twice_ReturnsEmptySecondTime() {
        // Arrange
        var builder = new DisposableBuilder();
        var d = Substitute.For<IDisposable>();
        builder.Add(d);

        // Act
        var first = builder.Build();
        var second = builder.Build();

        // Assert
        Assert.NotNull(first);
        Assert.Same(Disposable.Empty, second);
    }

    [Fact]
    public void Build_DoesNotDisposeUnderlyingUntilResultDisposed() {
        // Arrange
        var builder = new DisposableBuilder();
        var d1 = Substitute.For<IDisposable>();
        var d2 = Substitute.For<IDisposable>();
        builder.Add(d1);
        builder.Add(d2);

        // Act
        var combined = builder.Build();

        // Assert
        d1.DidNotReceive().Dispose();
        d2.DidNotReceive().Dispose();

        // Act
        combined.Dispose();

        // Assert
        Assert.All([d1, d2], d => d.Received(1).Dispose());
    }
}