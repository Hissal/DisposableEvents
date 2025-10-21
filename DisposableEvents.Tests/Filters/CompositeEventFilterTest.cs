using DisposableEvents;

namespace DisposableEvents.Tests.Filters;

[TestSubject(typeof(CompositeEventFilter<>))]
public class CompositeEventFilterTest {
    readonly IEventFilter<int>[] filters = Enumerable.Range(0, 3).Select(_ => Substitute.For<IEventFilter<int>>()).ToArray();
    CompositeEventFilter<int> Sut => CompositeEventFilter<int>.Create(filters);
    
    int message = 69;
    
    [Fact]
    public void Should_ReturnFilterResultPassed_WhenAllFiltersPass() {
        foreach (var filter in filters) {
            filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        }
        
        var result = Sut.Filter(ref message);
        result.Passed.Should().BeTrue();
    }
    
    [Fact]
    public void Should_ReturnFilterResultBlocked_WhenOneOrMoreBlock() {
        foreach (var filter in filters) {
            filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        }
        filters[1].Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);
        
        var result = Sut.Filter(ref message);
        result.Blocked.Should().BeTrue();
    }
    
    [Fact]
    public void FilterOrder_ShouldBeSetCorrectly() {
        var sut = CompositeEventFilter<int>.Create([], filterOrder: 3);
        sut.FilterOrder.Should().Be(3);
    }

    [Fact]
    public void FilterOrder_DefaultsToZero() {
        var sut  = CompositeEventFilter<int>.Create([]);
        sut.FilterOrder.Should().Be(0);
    }
    
    [Fact]
    public void Filters_ShouldReceiveMessage() {
        foreach (var filter in filters) {
            filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        }
        Sut.Filter(ref message);
        foreach (var filter in filters) {
            filter.Received(1).Filter(ref message);
        }
    }
    
    // ----- Filter Ordering ----- //
    
    [Fact]
    public void FiltersArray_StableSort_ShouldCallFiltersInCorrectOrder() {
        // Arrange
        const int c_generationCount = 3;
        var filterArray = Enumerable.Range(0, 1000).Select(_ => Substitute.For<IEventFilter<int>>()).ToArray();
        var shuffledFilters = GetShuffledFilters(filterArray, c_generationCount);
        var sut = CompositeEventFilter<int>.Create(shuffledFilters.ToArray(), FilterOrdering.StableSort);
        
        // Act
        sut.Filter(ref message);
        
        // Assert
        Received.InOrder(() => {
            foreach (var filter in filterArray) {
                filter.Filter(ref message);
            }
        });
    }
    
    
    [Fact]
    public void FiltersArray_UnstableSort_ShouldCallFiltersInGenerationalOrder() {
        // Arrange
        const int c_generationCount = 20;
        var filterArray = Enumerable.Range(0, 1000).Select(_ => Substitute.For<IEventFilter<int>>()).ToArray();
        var shuffledFilters = GetShuffledFilters(filterArray, c_generationCount, out var generations);
        var sut = CompositeEventFilter<int>.Create(shuffledFilters.ToArray(), FilterOrdering.UnstableSort);
        
        var callOrder = new List<int>();
        foreach (var generation in generations) {
            foreach (var filter in generation) {
                filter.When(f => f.Filter(ref Arg.Any<int>()))
                    .Do(_ => callOrder.Add(filter.FilterOrder));
            }
        }

        // Act
        sut.Filter(ref message);
        
        // Assert
        for (var gen = 0; gen < c_generationCount; gen++) {
            var nextGen = gen + 1;
            var lastCurrentGenIndex = callOrder.LastIndexOf(gen);
            var firstNextGenIndex = callOrder.FindIndex(x => x == nextGen);
            if (nextGen < c_generationCount) {
                lastCurrentGenIndex.Should().BeLessThan(firstNextGenIndex,
                    because: $"All filters of generation {gen} should be called before any filter of generation {nextGen}");
            }
        }
    }
    
    [Fact]
    public void FiltersArray_KeepOriginal_ShouldCallFiltersInOriginalOrder() {
        // Arrange
        var filterArray = Enumerable.Range(0, 100).Select(_ => Substitute.For<IEventFilter<int>>()).ToArray();
        var sut = CompositeEventFilter<int>.Create(filterArray.ToArray(), FilterOrdering.KeepOriginal);

        foreach (var filter in filterArray) {
            filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        }
        
        // Act
        sut.Filter(ref message);
        
        // Assert
        Received.InOrder(() => {
            foreach (var filter in filterArray) {
                filter.Filter(ref message);
            }
        });
    }
    
    // ----- Enumerable versions ----- //
    
    [Fact]
    public void FiltersEnumerable_StableSort_ShouldCallFiltersInCorrectOrder() {
        // Arrange
        const int c_generationCount = 3;
        var filterArray = Enumerable.Range(0, 1000).Select(_ => Substitute.For<IEventFilter<int>>()).ToArray();
        var shuffledFilters = GetShuffledFilters(filterArray, c_generationCount);
        var sut = CompositeEventFilter<int>.Create(shuffledFilters.ToArray().AsEnumerable(), FilterOrdering.StableSort);
        
        // Act
        sut.Filter(ref message);
        
        // Assert
        Received.InOrder(() => {
            foreach (var filter in filterArray) {
                filter.Filter(ref message);
            }
        });
    }
    
    [Fact]
    public void FiltersEnumerable_UnstableSort_ShouldCallFiltersInGenerationalOrder() {
        // Arrange
        const int c_generationCount = 20;
        var filterArray = Enumerable.Range(0, 1000).Select(_ => Substitute.For<IEventFilter<int>>()).ToArray();
        var shuffledFilters = GetShuffledFilters(filterArray, c_generationCount, out var generations);
        var sut = CompositeEventFilter<int>.Create(shuffledFilters.ToArray().AsEnumerable(), FilterOrdering.UnstableSort);
        
        var callOrder = new List<int>();
        foreach (var generation in generations) {
            foreach (var filter in generation) {
                filter.When(f => f.Filter(ref Arg.Any<int>()))
                    .Do(_ => callOrder.Add(filter.FilterOrder));
            }
        }

        // Act
        sut.Filter(ref message);
        
        // Assert
        for (var gen = 0; gen < c_generationCount; gen++) {
            var nextGen = gen + 1;
            var lastCurrentGenIndex = callOrder.LastIndexOf(gen);
            var firstNextGenIndex = callOrder.FindIndex(x => x == nextGen);
            if (nextGen < c_generationCount) {
                lastCurrentGenIndex.Should().BeLessThan(firstNextGenIndex,
                    because: $"All filters of generation {gen} should be called before any filter of generation {nextGen}");
            }
        }
    }
    
    [Fact]
    public void FiltersEnumerable_KeepOriginal_ShouldCallFiltersInOriginalOrder() {
        // Arrange
        var filterArray = Enumerable.Range(0, 100).Select(_ => Substitute.For<IEventFilter<int>>()).ToArray();
        var sut = CompositeEventFilter<int>.Create(filterArray.ToArray().AsEnumerable(), FilterOrdering.KeepOriginal);

        foreach (var filter in filterArray) {
            filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        }
        
        // Act
        sut.Filter(ref message);
        
        // Assert
        Received.InOrder(() => {
            foreach (var filter in filterArray) {
                filter.Filter(ref message);
            }
        });
    }
    
    // ----- Helpers ----- //
    
    [Pure]
    static IEventFilter<int>[] GetShuffledFilters(IEventFilter<int>[] filterArray, int generationCount) 
        => GetShuffledFilters(filterArray, generationCount, out _);
    [Pure]
    static IEventFilter<int>[] GetShuffledFilters(IEventFilter<int>[] filterArray, int generationCount, out IEventFilter<int>[][] generations) {
        var currentGeneration = 0;
        for (var i = 0; i < filterArray.Length; i++) {
            filterArray[i].FilterOrder.Returns(currentGeneration);
            filterArray[i].Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
            
            if (i % (filterArray.Length / generationCount) == filterArray.Length / generationCount - 1 && currentGeneration < generationCount - 1) {
                currentGeneration++;
            }
        }
        
        // Shuffle the entire generations as a block exchanging their positions
        // Stable sort should restore the original order keeping the relative order inside each generation
        // Unstable sort may mix the order inside each generation
        generations = new IEventFilter<int>[generationCount][];
        for (var i = 0; i < generationCount; i++) {
            generations[i] = filterArray.Where(f => f.FilterOrder == i).ToArray();
        }
        var shuffledGenerations = generations.OrderBy(_ => Guid.NewGuid()).ToArray();
        var shuffledFilters = shuffledGenerations.SelectMany(g => g).ToArray();
        return shuffledFilters;
    }
}