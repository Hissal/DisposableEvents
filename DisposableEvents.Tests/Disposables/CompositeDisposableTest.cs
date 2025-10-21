using DisposableEvents.Disposables;

namespace DisposableEvents.Tests.Disposables;

[TestSubject(typeof(CompositeDisposable))]
public class CompositeDisposableTest {
    [Fact]
    public void Ctor_NegativeCapacity_Throws() {
        var act = () => new CompositeDisposable(-1);
        act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("capacity");
    }

    [Fact]
    public void Ctor_WithCapacity_InitialCountIsZero() {
        var sut = new CompositeDisposable(4);
        sut.Count.Should().Be(0);
    }

    [Fact]
    public void Ctor_WithParams_AddsAllItems() {
        // Arrange
        var d1 = Substitute.For<IDisposable>();
        var d2 = Substitute.For<IDisposable>();

        // Act
        var sut = new CompositeDisposable(d1, d2);

        // Assert
        sut.Count.Should().Be(2);
        sut.Contains(d1).Should().BeTrue();
        sut.Contains(d2).Should().BeTrue();
    }

    [Fact]
    public void Ctor_WithEnumerable_AddsAllItems() {
        // Arrange
        var disposables = Enumerable.Range(0, 10).Select(x => Substitute.For<IDisposable>());
        var enumerable = disposables.ToList();

        // Act
        var sut = new CompositeDisposable(enumerable);

        // Assert
        sut.Count.Should().Be(10);
        Assert.All(enumerable, x => sut.Contains(x).Should().BeTrue());
    }

    [Fact]
    public void Add_AddsItem_IncrementsCount() {
        // Arrange
        var sut = new CompositeDisposable();
        var d1 = Substitute.For<IDisposable>();

        // Act
        sut.Add(d1);

        // Assert
        sut.Count.Should().Be(1);
        sut.Contains(d1).Should().BeTrue();
    }

    [Fact]
    public void Add_WhenDisposed_DisposesImmediately() {
        // Arrange
        var sut = new CompositeDisposable();
        var d = Substitute.For<IDisposable>();

        // Act
        sut.Dispose();
        sut.Add(d);

        // Assert
        d.Received(1).Dispose();
        sut.Count.Should().Be(0);
    }

    [Fact]
    public void Clear_DisposesAllAndEmptiesCollection() {
        // Arrange
        var sut = new CompositeDisposable();
        var d1 = Substitute.For<IDisposable>();
        var d2 = Substitute.For<IDisposable>();
        sut.Add(d1);
        sut.Add(d2);

        // Act
        sut.Clear();

        // Assert
        sut.Count.Should().Be(0);
        d1.Received(1).Dispose();
        d2.Received(1).Dispose();
        sut.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Clear_OnEmpty_DoesNothing() {
        // Arrange
        var sut = new CompositeDisposable();

        // Act
        sut.Clear();

        // Assert
        sut.Count.Should().Be(0);
        sut.IsDisposed.Should().BeFalse();
    }

    [Fact]
    public void Dispose_DisposesAllAndSetsIsDisposed() {
        // Arrange
        var sut = new CompositeDisposable();
        var d1 = Substitute.For<IDisposable>();
        var d2 = Substitute.For<IDisposable>();
        sut.Add(d1);
        sut.Add(d2);

        // Act
        sut.Dispose();

        // Assert
        sut.IsDisposed.Should().BeTrue();
        d1.Received(1).Dispose();
        d2.Received(1).Dispose();
        sut.Count.Should().Be(0);
    }

    [Fact]
    public void Dispose_IsIdempotent() {
        // Arrange
        var sut = new CompositeDisposable();
        var d = Substitute.For<IDisposable>();
        sut.Add(d);

        // Act
        sut.Dispose();
        sut.Dispose();

        // Assert
        d.Received(1).Dispose();
        sut.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Remove_RemovesItem_AndDoesNotDisposeIt() {
        // Arrange
        var sut = new CompositeDisposable();
        var d1 = Substitute.For<IDisposable>();
        var d2 = Substitute.For<IDisposable>();
        sut.Add(d1);
        sut.Add(d2);

        // Act
        var removed = sut.Remove(d1);

        // Assert
        removed.Should().BeTrue();
        sut.Count.Should().Be(1);
        sut.Contains(d1).Should().BeFalse();
        sut.Contains(d2).Should().BeTrue();
        d1.DidNotReceive().Dispose();
    }

    [Fact]
    public void Remove_MissingItem_ReturnsFalse() {
        // Arrange
        var sut = new CompositeDisposable();
        var d = Substitute.For<IDisposable>();

        // Act
        var removed = sut.Remove(d);

        // Assert
        removed.Should().BeFalse();
        sut.Count.Should().Be(0);
    }

    [Fact]
    public void Contains_ReturnsTrueForExistingItem() {
        // Arrange
        var sut = new CompositeDisposable();
        var d = Substitute.For<IDisposable>();
        sut.Add(d);
        
        // Assert
        sut.Contains(d).Should().BeTrue();
    }

    [Fact]
    public void IsReadOnly_IsFalse() {
        var sut = new CompositeDisposable();
        sut.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void CopyTo_CopiesItemsInOrderStartingAtIndex() {
        // Arrange
        var sut = new CompositeDisposable();
        var d1 = Substitute.For<IDisposable>();
        var d2 = Substitute.For<IDisposable>();
        var d3 = Substitute.For<IDisposable>();
        sut.Add(d1);
        sut.Add(d2);
        sut.Add(d3);
        var target = new IDisposable[5];

        // Act
        sut.CopyTo(target, 1);

        // Assert
        target[0].Should().BeNull();
        target[1].Should().Be(d1);
        target[2].Should().Be(d2);
        target[3].Should().Be(d3);
        target[4].Should().BeNull();
    }

    [Fact]
    public void Enumerator_EnumeratesAllItemsInOrder() {
        // Arrange
        var sut = new CompositeDisposable();
        var d1 = Substitute.For<IDisposable>();
        var d2 = Substitute.For<IDisposable>();
        sut.Add(d1);
        sut.Add(d2);

        // Act
        var items = sut.ToArray();

        // Assert
        items.Should().ContainInOrder(d1, d2);
    }
}