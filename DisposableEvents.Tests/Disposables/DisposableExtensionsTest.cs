using DisposableEvents.Disposables;

namespace DisposableEvents.Tests.Disposables;

[TestSubject(typeof(DisposableExtensions))]
public class DisposableExtensionsTest {
    [Fact]
    public void AddTo_Builder_AddsDisposable() {
        // Arrange
        var builder = Disposable.CreateBuilder();
        var disposable = Substitute.For<IDisposable>();

        // Act
        disposable.AddTo(ref builder);
        var combined = builder.Build();
        combined.Dispose();

        // Assert
        disposable.Received(1).Dispose();
    }
    
    [Fact]
    public void AddTo_Bag_AddsDisposable() {
        // Arrange
        var bag = Disposable.CreateBag(1);
        var disposable = Substitute.For<IDisposable>();

        // Act
        disposable.AddTo(ref bag);
        bag.Dispose();

        // Assert
        disposable.Received(1).Dispose();
    }
    
    [Fact]
    public void AddTo_ICollection_AddsDisposable() {
        // Arrange
        var collection = new List<IDisposable>();
        var disposable = Substitute.For<IDisposable>();

        // Act
        disposable.AddTo(collection);

        // Assert
        collection.Should().Contain(disposable);
    }
}