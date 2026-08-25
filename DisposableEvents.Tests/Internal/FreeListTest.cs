using DisposableEvents.Internal;

namespace DisposableEvents.Tests.Internal;

[TestSubject(typeof(FreeList<>))]
public class FreeListTest {
    readonly FreeList<string> sut = new(4);

    [Fact]
    public void Remove_WithCurrentKey_RemovesValueAndReturnsTrue() {
        // Arrange
        var key = sut.Add("value");

        // Act
        var removed = sut.Remove(key, false);

        // Assert
        removed.Should().BeTrue();
        sut.GetCount().Should().Be(0);
    }

    [Fact]
    public void Remove_WithKeyInvalidatedByClear_ReturnsFalse() {
        // Arrange
        var key = sut.Add("value");
        sut.Clear();

        // Act
        var removed = sut.Remove(key, false);

        // Assert
        removed.Should().BeFalse("a key from a previous generation no longer identifies a slot");
    }

    [Fact]
    public void Remove_WithKeyInvalidatedByClear_DoesNotRemoveValueReusingTheIndex() {
        // Arrange
        var staleKey = sut.Add("stale");
        sut.Clear();
        var newKey = sut.Add("new");
        newKey.Index.Should().Be(staleKey.Index, "the cleared index is expected to be handed out again");

        // Act
        sut.Remove(staleKey, false);

        // Assert
        sut.GetValue(newKey.Index).Should().Be("new");
        sut.GetCount().Should().Be(1);
    }

    [Fact]
    public void Add_AfterClear_ReturnsKeyWithNewGeneration() {
        // Arrange
        var staleKey = sut.Add("stale");
        sut.Clear();

        // Act
        var newKey = sut.Add("new");

        // Assert
        newKey.Generation.Should().NotBe(staleKey.Generation);
    }

    [Fact]
    public void Remove_WithAlreadyRemovedKeyOfCurrentGeneration_Throws() {
        // Arrange
        var key = sut.Add("value");
        sut.Remove(key, false);

        // Act
        var act = () => sut.Remove(key, false);

        // Assert
        act.Should().Throw<KeyNotFoundException>(
            "removing a key twice within one generation is a bug and must stay distinguishable from a stale key");
    }

    [Fact]
    public void Remove_WithKeyHandedOutBeforeAResize_RemovesTheCorrectValue() {
        // Arrange
        var first = sut.Add("first");
        for (var i = 0; i < 8; i++) {
            sut.Add($"filler {i}"); // Forces the backing array to grow past the initial capacity.
        }

        // Act
        var removed = sut.Remove(first, false);

        // Assert
        removed.Should().BeTrue("growing the list does not invalidate existing keys");
        sut.GetValue(first.Index).Should().BeNull();
    }

    [Fact]
    public void Remove_AfterDispose_ReturnsFalse() {
        // Arrange
        var key = sut.Add("value");
        sut.Dispose();

        // Act
        var removed = sut.Remove(key, false);

        // Assert
        removed.Should().BeFalse();
    }
}
