using DisposableEvents.Disposables;

namespace DisposableEvents.Tests.Disposables;

[TestSubject(typeof(DisposableBag))]
public class DisposableBagTest {
    DisposableBag bag;
    
    public DisposableBagTest() {
        bag = new DisposableBag(4);
    }
    
    [Fact]
    public void Clear_DisposesAll() {
        // Arrange
        var disposables = Enumerable.Range(0, 4).Select(_ => Substitute.For<IDisposable>()).ToArray();
        foreach (var disposable in disposables) {
            bag.Add(disposable);
        }

        // Act
        bag.Clear();

        // Assert
        Assert.All(disposables, d => d.Received(1).Dispose());
    }
    
    [Fact]
    public void Clear_MultipleTimes_DisposesAllOnlyOnce() {
        // Arrange
        var disposables = Enumerable.Range(0, 4).Select(_ => Substitute.For<IDisposable>()).ToArray();
        foreach (var disposable in disposables) {
            bag.Add(disposable);
        }

        // Act
        bag.Clear();
        bag.Clear();
        bag.Clear();

        // Assert
        Assert.All(disposables, d => d.Received(1).Dispose());
    }
    
    [Fact]
    public void AllowsAddingAfterClear() {
        // Arrange
        var d1 = Substitute.For<IDisposable>();

        // Act
        bag.Clear();
        bag.Add(d1);
        bag.Clear();

        // Assert
        d1.Received(1).Dispose();
    }

    [Fact]
    public void Dispose_DisposesAll() {
        // Arrange
        var disposables = Enumerable.Range(0, 4).Select(_ => Substitute.For<IDisposable>()).ToArray();
        foreach (var disposable in disposables) {
            bag.Add(disposable);
        }

        // Act
        bag.Dispose();

        // Assert
        Assert.All(disposables, d => d.Received(1).Dispose());
    }

    [Fact]
    public void Dispose_MultipleTimes_DisposesOnlyOnce() {
        // Arrange
        var disposables = Enumerable.Range(0, 4).Select(_ => Substitute.For<IDisposable>()).ToArray();
        foreach (var disposable in disposables) {
            bag.Add(disposable);
        }

        // Act
        bag.Dispose();
        bag.Dispose();
        bag.Dispose();

        // Assert
        Assert.All(disposables, d => d.Received(1).Dispose());
    }

    [Fact]
    public void Add_AfterDisposeImmediatelyDisposes() {
        // Arrange
        var d1 = Substitute.For<IDisposable>();

        // Act
        bag.Dispose();
        bag.Add(d1);

        // Assert
        d1.Received(1).Dispose();
    }
    
    [Fact]
    public void Add_ManyDisposables_WorksCorrectly() {
        // Arrange
        var disposables = Enumerable.Range(0, 1000).Select(_ => Substitute.For<IDisposable>()).ToArray();

        // Act
        foreach (var disposable in disposables) {
            bag.Add(disposable);
        }
        bag.Dispose();

        // Assert
        Assert.All(disposables, d => d.Received(1).Dispose());
    }
}